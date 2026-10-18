using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbaDeskBack.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketAttachmentBinary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentContentType",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AttachmentData",
                table: "Tickets",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "Tickets",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentContentType",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AttachmentData",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "Tickets");
        }
    }
}
