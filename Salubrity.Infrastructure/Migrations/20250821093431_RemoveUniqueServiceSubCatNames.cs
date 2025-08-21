using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueServiceSubCatNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_Name",
                table: "ServiceCategories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_Name",
                table: "ServiceCategories",
                column: "Name",
                unique: true);
        }
    }
}
