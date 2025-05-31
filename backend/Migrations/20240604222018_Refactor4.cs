using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Refactor4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Website",
                table: "Cinema",
                newName: "Url");

            migrationBuilder.AddColumn<string>(
                name: "ShopUrl",
                table: "Cinema",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopUrl",
                table: "Cinema");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Cinema",
                newName: "Website");
        }
    }
}
