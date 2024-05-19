using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kinohannover.Migrations
{
    /// <inheritdoc />
    public partial class AddCinemaColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Time",
                table: "ShowTime",
                newName: "StartTime");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Cinema",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Cinema");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "ShowTime",
                newName: "Time");
        }
    }
}
