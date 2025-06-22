using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Refactor3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alias_Movies_MovieId",
                table: "Alias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Alias",
                table: "Alias");

            migrationBuilder.DropColumn(
                name: "ShopUrl",
                table: "ShowTime");

            migrationBuilder.RenameTable(
                name: "Alias",
                newName: "Aliases");

            migrationBuilder.RenameIndex(
                name: "IX_Alias_MovieId",
                table: "Aliases",
                newName: "IX_Aliases_MovieId");

            migrationBuilder.AlterColumn<int>(
                name: "MovieId",
                table: "Aliases",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Aliases_Movies_MovieId",
                table: "Aliases",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aliases_Movies_MovieId",
                table: "Aliases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases");

            migrationBuilder.RenameTable(
                name: "Aliases",
                newName: "Alias");

            migrationBuilder.RenameIndex(
                name: "IX_Aliases_MovieId",
                table: "Alias",
                newName: "IX_Alias_MovieId");

            migrationBuilder.AddColumn<string>(
                name: "ShopUrl",
                table: "ShowTime",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MovieId",
                table: "Alias",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Alias",
                table: "Alias",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alias_Movies_MovieId",
                table: "Alias",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id");
        }
    }
}
