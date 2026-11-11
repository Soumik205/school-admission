using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Domain;

namespace SchoolAdmission.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Hold> Holds => Set<Hold>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SchoolClass>(entity =>
        {
            entity.ToTable("classes", t => t.HasCheckConstraint("ck_classes_total_seats_positive", "total_seats > 0"));
            entity.Property(c => c.Id).ValueGeneratedNever();
            entity.Property(c => c.Name).IsRequired();
        });

        modelBuilder.Entity<Hold>(entity =>
        {
            entity.ToTable("holds", t =>
            {
                t.HasCheckConstraint("ck_holds_student_id_format", "student_id ~ '^[0-9]{17}$'");
                t.HasCheckConstraint("ck_holds_status", "status IN ('HELD', 'CONFIRMED', 'EXPIRED', 'CANCELLED')");
            });
            entity.Property(h => h.Id).ValueGeneratedNever();
            entity.Property(h => h.StudentId).HasMaxLength(17).IsRequired();

            // Stored as the same uppercase strings the API returns, so raw SQL and the data read the same way.
            entity.Property(h => h.Status).HasConversion(
                status => status.ToString().ToUpperInvariant(),
                value => Enum.Parse<HoldStatus>(value, true));

            entity.HasOne(h => h.Class)
                .WithMany()
                .HasForeignKey(h => h.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // Database-level guard for one active or confirmed application per student per class.
            entity.HasIndex(h => new { h.ClassId, h.StudentId })
                .IsUnique()
                .HasFilter("status IN ('HELD', 'CONFIRMED')")
                .HasDatabaseName("ux_holds_class_student_active");

            entity.HasIndex(h => new { h.ClassId, h.Status });

            entity.HasIndex(h => new { h.StudentId, h.CreatedAt })
                .IsDescending(false, true);

            entity.HasIndex(h => h.ExpiresAt)
                .HasFilter("status = 'HELD'")
                .HasDatabaseName("ix_holds_expires_at_held");
        });
    }
}
