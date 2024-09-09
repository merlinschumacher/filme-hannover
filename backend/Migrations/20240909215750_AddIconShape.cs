using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kinohannover.Migrations
{
    /// <inheritdoc />
    public partial class AddIconShape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconClass",
                table: "Cinema",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconClass",
                table: "Cinema");
        }
    }
}
