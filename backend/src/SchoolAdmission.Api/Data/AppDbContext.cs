using Microsoft.EntityFrameworkCore;

namespace SchoolAdmission.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
