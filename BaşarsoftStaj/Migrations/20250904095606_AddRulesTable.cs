using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BaşarsoftStaj.Migrations
{
    /// <inheritdoc />
    public partial class AddRulesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PointsEF",
                table: "PointsEF");

            migrationBuilder.RenameTable(
                name: "PointsEF",
                newName: "Shapes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shapes",
                table: "Shapes",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    GeometryType = table.Column<string>(type: "text", nullable: false),
                    ShapeType = table.Column<string>(type: "text", nullable: false),
                    RelatedGeometryType = table.Column<string>(type: "text", nullable: false),
                    RelatedShapeType = table.Column<string>(type: "text", nullable: false),
                    ValidationType = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Shapes",
                table: "Shapes");

            migrationBuilder.RenameTable(
                name: "Shapes",
                newName: "PointsEF");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointsEF",
                table: "PointsEF",
                column: "Id");
        }
    }
}
