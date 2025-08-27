using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PentaBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardColumns_Boards_BoardId1",
                table: "BoardColumns");

            migrationBuilder.DropIndex(
                name: "IX_BoardColumns_BoardId1",
                table: "BoardColumns");

            migrationBuilder.DropColumn(
                name: "BoardId1",
                table: "BoardColumns");

            migrationBuilder.CreateTable(
                name: "WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardColumnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Task"),
                    Priority = table.Column<byte>(type: "tinyint", nullable: true),
                    Severity = table.Column<byte>(type: "tinyint", nullable: true),
                    ReporterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoryPoints = table.Column<int>(type: "int", nullable: true),
                    OriginalEstimateHours = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    RemainingHours = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    CompletedHours = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TagsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderKey = table.Column<int>(type: "int", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItems_BoardColumns_BoardColumnId",
                        column: x => x.BoardColumnId,
                        principalTable: "BoardColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkItems_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_Users_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkItems_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_AssigneeId",
                table: "WorkItems",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_BoardColumnId_OrderKey",
                table: "WorkItems",
                columns: new[] { "BoardColumnId", "OrderKey" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_BoardId",
                table: "WorkItems",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId",
                table: "WorkItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ReporterId",
                table: "WorkItems",
                column: "ReporterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkItems");

            migrationBuilder.AddColumn<Guid>(
                name: "BoardId1",
                table: "BoardColumns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_BoardId1",
                table: "BoardColumns",
                column: "BoardId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardColumns_Boards_BoardId1",
                table: "BoardColumns",
                column: "BoardId1",
                principalTable: "Boards",
                principalColumn: "Id");
        }
    }
}
