using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kinohannover.Migrations
{
    /// <inheritdoc />
    public partial class AddShowTimeUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "ShowTime",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "ShowTime");
        }
    }
}
