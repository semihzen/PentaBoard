using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PentaBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardAndColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DefaultColumnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DoneColumnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Boards_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Boards_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BoardColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderKey = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WipLimit = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDoneLike = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    BoardId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardColumns_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardColumns_Boards_BoardId1",
                        column: x => x.BoardId1,
                        principalTable: "Boards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_BoardId_Name",
                table: "BoardColumns",
                columns: new[] { "BoardId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_BoardId_OrderKey",
                table: "BoardColumns",
                columns: new[] { "BoardId", "OrderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_BoardId1",
                table: "BoardColumns",
                column: "BoardId1");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_CreatedById",
                table: "Boards",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_ProjectId",
                table: "Boards",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardColumns");

            migrationBuilder.DropTable(
                name: "Boards");
        }
    }
}
