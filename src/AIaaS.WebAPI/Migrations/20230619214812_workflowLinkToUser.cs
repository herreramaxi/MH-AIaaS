using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class workflowLinkToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Workflows",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_UserId",
                table: "Workflows",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workflows_Users_UserId",
                table: "Workflows",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workflows_Users_UserId",
                table: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_Workflows_UserId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Workflows");
        }
    }
}
