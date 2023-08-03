using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tikkit_SolpacWeb.Migrations
{
    public partial class mstrr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Partners",
                newName: "PartnerID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PartnerID",
                table: "Partners",
                newName: "ID");
        }
    }
}
