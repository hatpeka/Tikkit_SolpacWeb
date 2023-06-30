using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tikkit_SolpacWeb.Migrations
{
    public partial class EditRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubjectOfRequest",
                table: "Requests");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubjectOfRequest",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
