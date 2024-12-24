using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eBookLibraryService.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountUntilToBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountUntil",
                table: "Books",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountUntil",
                table: "Books");
        }
    }
}
