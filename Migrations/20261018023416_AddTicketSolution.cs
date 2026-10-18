using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbaDeskBack.Migrations
{
    public partial class AddTicketSolution : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Solution",
                table: "Tickets",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Solution",
                table: "Tickets");
        }
    }
}
