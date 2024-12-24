using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eBookLibraryService.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountAndMultiPublishers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "DiscountPrice",
                table: "Books",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicationYears",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Publishers",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPrice",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "PublicationYears",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Publishers",
                table: "Books");
        }
    }
}
