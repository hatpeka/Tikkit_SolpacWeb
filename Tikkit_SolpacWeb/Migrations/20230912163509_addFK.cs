using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tikkit_SolpacWeb.Migrations
{
    public partial class addFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Supporter",
                table: "Requests",
                newName: "SupporterName");

            migrationBuilder.RenameColumn(
                name: "RequestPerson",
                table: "Requests",
                newName: "RequestPersonName");

            migrationBuilder.AlterColumn<int>(
                name: "SupporterID",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RequestPersonID",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Target",
                table: "Notification",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestPersonID",
                table: "Requests",
                column: "RequestPersonID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_SupporterID",
                table: "Requests",
                column: "SupporterID");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RequestID",
                table: "Notification",
                column: "RequestID");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Target",
                table: "Notification",
                column: "Target");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Requests_RequestID",
                table: "Notification",
                column: "RequestID",
                principalTable: "Requests",
                principalColumn: "RequestNo",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Users_Target",
                table: "Notification",
                column: "Target",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_RequestPersonID",
                table: "Requests",
                column: "RequestPersonID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_SupporterID",
                table: "Requests",
                column: "SupporterID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Requests_RequestID",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Users_Target",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_RequestPersonID",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_SupporterID",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_RequestPersonID",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_SupporterID",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Notification_RequestID",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_Target",
                table: "Notification");

            migrationBuilder.RenameColumn(
                name: "SupporterName",
                table: "Requests",
                newName: "Supporter");

            migrationBuilder.RenameColumn(
                name: "RequestPersonName",
                table: "Requests",
                newName: "RequestPerson");

            migrationBuilder.AlterColumn<int>(
                name: "SupporterID",
                table: "Requests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "RequestPersonID",
                table: "Requests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Target",
                table: "Notification",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
