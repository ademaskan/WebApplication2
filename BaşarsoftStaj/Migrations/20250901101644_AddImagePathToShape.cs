using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaşarsoftStaj.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToShape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "PointsEF",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "PointsEF");
        }
    }
}
