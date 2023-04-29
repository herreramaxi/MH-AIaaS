using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDataColumnIntoWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Workflows",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "Workflows");
        }
    }
}
