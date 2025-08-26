using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaşarsoftStaj.Migrations
{
    /// <inheritdoc />
    public partial class GeometryType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WKT",
                table: "PointsEF");

            migrationBuilder.RenameColumn(
                name: "WellKnownText",
                table: "PointsEF",
                newName: "Geometry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Geometry",
                table: "PointsEF",
                newName: "WellKnownText");

            migrationBuilder.AddColumn<string>(
                name: "WKT",
                table: "PointsEF",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }
    }
}
