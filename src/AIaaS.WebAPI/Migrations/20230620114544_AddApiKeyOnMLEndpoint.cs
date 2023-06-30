using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyOnMLEndpoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Endpoints",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Endpoints");
        }
    }
}
