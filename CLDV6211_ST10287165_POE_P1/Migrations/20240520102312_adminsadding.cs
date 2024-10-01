using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLDV6211_ST10287165_POE_P1.Migrations
{
    /// <inheritdoc />
    public partial class adminsadding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustPasswordHash",
                table: "Admins",
                newName: "AdminPasswordHash");

            migrationBuilder.RenameColumn(
                name: "CustEmail",
                table: "Admins",
                newName: "AdminEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdminPasswordHash",
                table: "Admins",
                newName: "CustPasswordHash");

            migrationBuilder.RenameColumn(
                name: "AdminEmail",
                table: "Admins",
                newName: "CustEmail");
        }
    }
}
