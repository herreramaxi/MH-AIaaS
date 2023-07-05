using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeTypeToWorkflowDataView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NodeType",
                table: "WorkflowDataView",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NodeType",
                table: "WorkflowDataView");
        }
    }
}
