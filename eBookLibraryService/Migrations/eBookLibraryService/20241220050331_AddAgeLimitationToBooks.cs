using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eBookLibraryService.Migrations.eBookLibraryService
{
    /// <inheritdoc />
    public partial class AddAgeLimitationToBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgeLimitation",
                table: "Books",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeLimitation",
                table: "Books");
        }
    }
}
