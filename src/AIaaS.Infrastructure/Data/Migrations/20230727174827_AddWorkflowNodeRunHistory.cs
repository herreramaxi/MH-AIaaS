using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowNodeRunHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowNodeRunHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowRunHistoryId = table.Column<int>(type: "int", nullable: false),
                    NodeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalMilliseconds = table.Column<double>(type: "float", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowNodeRunHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowNodeRunHistory_WorkflowRunHistory_WorkflowRunHistoryId",
                        column: x => x.WorkflowRunHistoryId,
                        principalTable: "WorkflowRunHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodeRunHistory_WorkflowRunHistoryId",
                table: "WorkflowNodeRunHistory",
                column: "WorkflowRunHistoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowNodeRunHistory");
        }
    }
}
