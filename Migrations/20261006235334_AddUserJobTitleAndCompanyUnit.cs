using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbaDeskBack.Migrations
{
    /// <inheritdoc />
    public partial class AddUserJobTitleAndCompanyUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyUnit",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyUnit",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Users");
        }
    }
}
