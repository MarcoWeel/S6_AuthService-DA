using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace authservice.dataaccess.Migrations
{
    public partial class change_verified_to_acknowledged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Verified",
                table: "User",
                newName: "Acknowledged");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Acknowledged",
                table: "User",
                newName: "Verified");
        }
    }
}
