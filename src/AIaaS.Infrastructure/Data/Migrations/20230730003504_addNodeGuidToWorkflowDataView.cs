using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addNodeGuidToWorkflowDataView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "WorkflowDataViews");

            migrationBuilder.AddColumn<Guid>(
                name: "NodeGuid",
                table: "WorkflowDataViews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NodeGuid",
                table: "WorkflowDataViews");

            migrationBuilder.AddColumn<string>(
                name: "NodeId",
                table: "WorkflowDataViews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
