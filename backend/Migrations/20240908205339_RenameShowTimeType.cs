using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kinohannover.Migrations
{
    /// <inheritdoc />
    public partial class RenameShowTimeType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ShowTime",
                newName: "DubType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DubType",
                table: "ShowTime",
                newName: "Type");
        }
    }
}
