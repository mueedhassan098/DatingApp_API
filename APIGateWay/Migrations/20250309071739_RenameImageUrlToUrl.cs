using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGateWay.Migrations
{
    /// <inheritdoc />
    public partial class RenameImageUrlToUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Photos",
                newName: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Photos",
                newName: "ImageUrl");
        }
    }
}
