using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PentaBoard.Api.Migrations
{
    public partial class RemoveOwnerHandleFromProjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eğer DB'de varsa kaldır
            migrationBuilder.DropColumn(
                name: "OwnerHandle",
                table: "Projects");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alınırsa eski haline getir (nullable yapmadan istersen NOT NULL + default verebilirsin)
            migrationBuilder.AddColumn<string>(
                name: "OwnerHandle",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
