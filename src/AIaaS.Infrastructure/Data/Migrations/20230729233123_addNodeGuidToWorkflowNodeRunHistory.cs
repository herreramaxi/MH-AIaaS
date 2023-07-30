using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addNodeGuidToWorkflowNodeRunHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "WorkflowNodeRunHistory");

            migrationBuilder.AddColumn<Guid>(
                name: "NodeGuid",
                table: "WorkflowNodeRunHistory",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NodeGuid",
                table: "WorkflowNodeRunHistory");

            migrationBuilder.AddColumn<string>(
                name: "NodeId",
                table: "WorkflowNodeRunHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
