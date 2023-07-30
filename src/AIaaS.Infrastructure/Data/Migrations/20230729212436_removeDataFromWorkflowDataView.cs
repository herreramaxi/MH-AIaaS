using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeDataFromWorkflowDataView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "WorkflowDataViews");

            migrationBuilder.AddColumn<string>(
                name: "S3Key",
                table: "WorkflowDataViews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "S3Key",
                table: "WorkflowDataViews");

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "WorkflowDataViews",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
