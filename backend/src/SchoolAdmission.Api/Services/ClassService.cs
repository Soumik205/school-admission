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

        return new ClassResponse(schoolClass.Id, schoolClass.Name, schoolClass.TotalSeats, 0, 0, schoolClass.TotalSeats);
    }
}
