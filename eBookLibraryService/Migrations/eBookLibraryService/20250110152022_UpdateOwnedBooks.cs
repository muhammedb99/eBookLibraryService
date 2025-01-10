using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eBookLibraryService.Migrations.eBookLibraryService
{
    /// <inheritdoc />
    public partial class UpdateOwnedBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BorrowDueDate",
                table: "OwnedBooks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BorrowDueDate",
                table: "OwnedBooks");
        }
    }
}
