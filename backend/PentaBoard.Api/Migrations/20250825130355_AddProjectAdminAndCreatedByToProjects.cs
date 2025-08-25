using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PentaBoard.Api.Migrations
{
    public partial class AddProjectAdminAndCreatedByToProjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SADECE yeni kolonlar: nullable + defaultsuz
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectAdminId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectAdminId",
                table: "Projects",
                column: "ProjectAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_ProjectAdminId",
                table: "Projects",
                column: "ProjectAdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_ProjectAdminId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectAdminId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectAdminId",
                table: "Projects");
        }
    }
}
