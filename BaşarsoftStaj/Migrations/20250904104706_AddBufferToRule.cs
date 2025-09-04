using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaşarsoftStaj.Migrations
{
    /// <inheritdoc />
    public partial class AddBufferToRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Buffer",
                table: "Rules",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Buffer",
                table: "Rules");
        }
    }
}
