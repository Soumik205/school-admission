using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolAdmission.Api.Contracts;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Errors;

namespace SchoolAdmission.Api.Services;

public class HoldService(AppDbContext db, TimeProvider time, HoldSettings settings)
{
    public async Task<HoldCreatedResponse> CreateAsync(string classId, string studentId)
    {
        if (!Guid.TryParse(classId, out var id))
        {
            throw ClassService.NotFound();
        }

        try
        {
            return await CreateLockedAsync(id, studentId);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: AppDbContext.ActiveHoldUniqueIndex,
        })
        {
            throw AlreadyApplied();
        }
    }

    private async Task<HoldCreatedResponse> CreateLockedAsync(Guid classId, string studentId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        // Every hold insert for a class takes this row lock first, so inserts for one class run one at a
        // time inside Postgres. That keeps held + confirmed within total_seats across any number of
        // backend processes. Isolation must stay READ COMMITTED so the count below sees the previous
        // lock holder's committed insert.
        var locked = await db.Classes
            .FromSql($"SELECT * FROM classes WHERE id = {classId} FOR UPDATE")
            .ToListAsync();
        var schoolClass = locked.SingleOrDefault() ?? throw ClassService.NotFound();

        var now = time.GetUtcNow().UtcDateTime;

        // Expire stale holds first so the partial unique index lets a student apply again right after
        // an earlier hold ran out, even before the background sweeper has run.
        await db.Holds
            .Where(h => h.ClassId == classId && h.Status == HoldStatus.Held && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.Status, HoldStatus.Expired)
                .SetProperty(h => h.UpdatedAt, now));

        var active = db.Holds.Where(h => h.ClassId == classId
            && (h.Status == HoldStatus.Held || h.Status == HoldStatus.Confirmed));

        if (await active.AnyAsync(h => h.StudentId == studentId))
        {
            throw AlreadyApplied();
        }

        if (await active.CountAsync() >= schoolClass.TotalSeats)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.ClassFull,
                "No seats available", $"{schoolClass.Name} has no available seats.");
        }

        var hold = new Hold
        {
            Id = Guid.CreateVersion7(),
            ClassId = classId,
            StudentId = studentId,
            Status = HoldStatus.Held,
            ExpiresAt = now + settings.HoldDuration,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.Holds.Add(hold);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new HoldCreatedResponse(hold.Id, classId, schoolClass.Name, studentId, hold.Status,
            hold.ExpiresAt, hold.CreatedAt);
    }

    public async Task<HoldResponse> GetAsync(string holdId)
    {
        var hold = Guid.TryParse(holdId, out var id)
            ? await db.Holds.AsNoTracking().Include(h => h.Class).SingleOrDefaultAsync(h => h.Id == id)
            : null;
        return ToResponse(hold ?? throw HoldNotFound());
    }

    private HoldResponse ToResponse(Hold hold) => new(hold.Id, hold.ClassId, hold.Class.Name, hold.StudentId,
        hold.EffectiveStatus(time.GetUtcNow().UtcDateTime), hold.ExpiresAt, hold.CreatedAt, hold.ConfirmedAt);

    private static ApiException HoldNotFound() => new(StatusCodes.Status404NotFound, ErrorCodes.HoldNotFound,
        "Application not found", "This application could not be found.");

    private static ApiException AlreadyApplied() => new(StatusCodes.Status409Conflict, ErrorCodes.AlreadyApplied,
        "Already applied", "This student already has an active application or admission in this class.");
}
