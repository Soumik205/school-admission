using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolAdmission.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueClassName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF Core cannot model an expression index, so the case-insensitive uniqueness is plain SQL.
            migrationBuilder.Sql("CREATE UNIQUE INDEX ux_classes_name_lower ON classes (lower(name));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ux_classes_name_lower;");
        }
    }
}
