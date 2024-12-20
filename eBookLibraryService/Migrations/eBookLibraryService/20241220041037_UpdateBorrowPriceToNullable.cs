using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eBookLibraryService.Migrations.eBookLibraryService
{
    /// <inheritdoc />
    public partial class UpdateBorrowPriceToNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "BorrowPrice",
                table: "Books",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "BorrowPrice",
                table: "Books",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);
        }
    }
}
