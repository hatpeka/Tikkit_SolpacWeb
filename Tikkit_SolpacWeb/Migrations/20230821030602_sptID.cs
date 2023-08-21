using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tikkit_SolpacWeb.Migrations
{
    public partial class sptID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupporterID",
                table: "Requests",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupporterID",
                table: "Requests");
        }
    }
}
