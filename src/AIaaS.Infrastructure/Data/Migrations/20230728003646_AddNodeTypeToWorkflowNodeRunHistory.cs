using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeTypeToWorkflowNodeRunHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NodeType",
                table: "WorkflowNodeRunHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NodeType",
                table: "WorkflowNodeRunHistory");
        }
    }
}
