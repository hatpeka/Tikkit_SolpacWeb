using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tikkit_SolpacWeb.Migrations
{
    public partial class Cretee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatePerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Supporter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Partner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectOfRequest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentsOfRequest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestNo);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Partner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RePassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
