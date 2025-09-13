using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLabFormToIntakeForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLabForm",
                table: "IntakeForms",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLabForm",
                table: "IntakeForms");
        }
    }
}
