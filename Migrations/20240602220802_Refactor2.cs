using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kinohannover.Migrations
{
    /// <inheritdoc />
    public partial class Refactor2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aliases",
                table: "Movies");

            migrationBuilder.RenameColumn(
                name: "LinkToShop",
                table: "Cinema",
                newName: "HasShop");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Movies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Alias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alias_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alias_MovieId",
                table: "Alias",
                column: "MovieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alias");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Movies");

            migrationBuilder.RenameColumn(
                name: "HasShop",
                table: "Cinema",
                newName: "LinkToShop");

            migrationBuilder.AddColumn<string>(
                name: "Aliases",
                table: "Movies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
