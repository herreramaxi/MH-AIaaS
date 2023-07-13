using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowDataViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowDataView_Workflows_WorkflowId",
                table: "WorkflowDataView");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowDataView",
                table: "WorkflowDataView");

            migrationBuilder.RenameTable(
                name: "WorkflowDataView",
                newName: "WorkflowDataViews");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowDataView_WorkflowId",
                table: "WorkflowDataViews",
                newName: "IX_WorkflowDataViews_WorkflowId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowDataViews",
                table: "WorkflowDataViews",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowDataViews_Workflows_WorkflowId",
                table: "WorkflowDataViews",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowDataViews_Workflows_WorkflowId",
                table: "WorkflowDataViews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowDataViews",
                table: "WorkflowDataViews");

            migrationBuilder.RenameTable(
                name: "WorkflowDataViews",
                newName: "WorkflowDataView");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowDataViews_WorkflowId",
                table: "WorkflowDataView",
                newName: "IX_WorkflowDataView_WorkflowId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowDataView",
                table: "WorkflowDataView",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowDataView_Workflows_WorkflowId",
                table: "WorkflowDataView",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
