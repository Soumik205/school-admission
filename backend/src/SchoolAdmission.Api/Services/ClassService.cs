using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolAdmission.Api.Contracts;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Errors;

namespace SchoolAdmission.Api.Services;

public class ClassService(AppDbContext db, TimeProvider time)
{
    public async Task<ClassResponse> CreateAsync(CreateClassRequest request)
    {
        var schoolClass = new SchoolClass
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name!.Trim(),
            TotalSeats = request.TotalSeats!.Value,
            CreatedAt = time.GetUtcNow().UtcDateTime,
        };
        db.Classes.Add(schoolClass);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: AppDbContext.ClassNameUniqueIndex,
        })
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.ClassNameTaken,
                "Class name taken", $"A class named '{schoolClass.Name}' already exists.");
        }

        return ToResponse(schoolClass.Id, schoolClass.Name, schoolClass.TotalSeats, 0, 0);
    }

    public async Task<List<ClassResponse>> ListAsync()
    {
        var rows = await QueryWithCounts(db.Classes.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)).ToListAsync();
        return rows.Select(r => ToResponse(r.Id, r.Name, r.TotalSeats, r.Held, r.Confirmed)).ToList();
    }

    public async Task<ClassResponse> GetAsync(string classId)
    {
        var row = Guid.TryParse(classId, out var id)
            ? await QueryWithCounts(db.Classes.Where(c => c.Id == id)).SingleOrDefaultAsync()
            : null;
        if (row is null)
        {
            throw NotFound();
        }
        return ToResponse(row.Id, row.Name, row.TotalSeats, row.Held, row.Confirmed);
    }

    public static ApiException NotFound() => new(StatusCodes.Status404NotFound, ErrorCodes.ClassNotFound,
        "Class not found", "This class could not be found.");

    // Counts are computed from holds at read time, so a hold stops using its seat the moment it expires.
    private IQueryable<ClassCounts> QueryWithCounts(IQueryable<SchoolClass> classes)
    {
        var now = time.GetUtcNow().UtcDateTime;
        return classes.Select(c => new ClassCounts(
            c.Id,
            c.Name,
            c.TotalSeats,
            db.Holds.Count(h => h.ClassId == c.Id && h.Status == HoldStatus.Held && h.ExpiresAt > now),
            db.Holds.Count(h => h.ClassId == c.Id && h.Status == HoldStatus.Confirmed)));
    }

    private static ClassResponse ToResponse(Guid id, string name, int totalSeats, int held, int confirmed) =>
        new(id, name, totalSeats, held, confirmed, totalSeats - held - confirmed);

    private record ClassCounts(Guid Id, string Name, int TotalSeats, int Held, int Confirmed);
}
