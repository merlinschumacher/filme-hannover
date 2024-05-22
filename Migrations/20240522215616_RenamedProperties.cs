using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kinohannover.Migrations
{
    /// <inheritdoc />
    public partial class RenamedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShowTimeType",
                table: "ShowTime",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "ShowTimeLanguage",
                table: "ShowTime",
                newName: "Language");

            migrationBuilder.AddColumn<string>(
                name: "ShopUrl",
                table: "ShowTime",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopUrl",
                table: "ShowTime");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ShowTime",
                newName: "ShowTimeType");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "ShowTime",
                newName: "ShowTimeLanguage");
        }
    }
}
