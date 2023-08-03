using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tikkit_SolpacWeb.Migrations
{
    public partial class usid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerID",
                table: "Users",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartnerID",
                table: "Users");
        }
    }
}
