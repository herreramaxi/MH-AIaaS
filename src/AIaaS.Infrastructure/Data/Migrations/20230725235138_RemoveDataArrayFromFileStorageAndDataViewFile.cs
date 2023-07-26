using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDataArrayFromFileStorageAndDataViewFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "FileStorages");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "DataViewFiles");

            migrationBuilder.AddColumn<string>(
                name: "S3Key",
                table: "FileStorages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "S3Key",
                table: "DataViewFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "S3Key",
                table: "FileStorages");

            migrationBuilder.DropColumn(
                name: "S3Key",
                table: "DataViewFiles");

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "FileStorages",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "DataViewFiles",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
