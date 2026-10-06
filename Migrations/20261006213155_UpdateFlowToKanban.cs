using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbaDeskBack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFlowToKanban : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_HomologationResponsibleUserId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "Homologations");

            migrationBuilder.DropTable(
                name: "TestCases");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_HomologationResponsibleUserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "HomologationResponsibleUserId",
                table: "Tickets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HomologationResponsibleUserId",
                table: "Tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Homologations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homologations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Homologations_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Homologations_Users_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Homologations_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Homologations_Users_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualResult = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ExpectedResult = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCases_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestCases_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_HomologationResponsibleUserId",
                table: "Tickets",
                column: "HomologationResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Homologations_DecidedByUserId",
                table: "Homologations",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Homologations_RequestedByUserId",
                table: "Homologations",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Homologations_ResponsibleUserId",
                table: "Homologations",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Homologations_TicketId",
                table: "Homologations",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_CreatedByUserId",
                table: "TestCases",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_TicketId",
                table: "TestCases",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_HomologationResponsibleUserId",
                table: "Tickets",
                column: "HomologationResponsibleUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
