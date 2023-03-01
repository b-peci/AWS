using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWS.Migrations
{
    public partial class RenamedImagetoImageKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Posts");

            migrationBuilder.AddColumn<string>(
                name: "ImageKey",
                table: "Posts",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageKey",
                table: "Posts");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
