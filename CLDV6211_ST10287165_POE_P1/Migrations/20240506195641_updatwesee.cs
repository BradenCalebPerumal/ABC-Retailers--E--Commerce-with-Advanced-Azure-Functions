using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLDV6211_ST10287165_POE_P1.Migrations
{
    /// <inheritdoc />
    public partial class updatwesee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Client",
                keyColumn: "ClientId",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "Client",
                keyColumn: "ClientId",
                keyValue: 2,
                columns: new[] { "CellNum", "ClientFirstName", "Email", "IdentityNum", "LastName", "Password", "Username" },
                values: new object[] { "123-456-7890", "John", "john.doe@example.com", "1234567890", "Doe", "password1", "client1" });

            migrationBuilder.UpdateData(
                table: "Client",
                keyColumn: "ClientId",
                keyValue: 3,
                columns: new[] { "CellNum", "ClientFirstName", "Email", "IdentityNum", "LastName", "Password", "Username" },
                values: new object[] { "987-654-3210", "Jane", "jane.smith@example.com", "0987654321", "Smith", "password2", "client2" });

            migrationBuilder.UpdateData(
                table: "Client",
                keyColumn: "ClientId",
                keyValue: 4,
                columns: new[] { "CellNum", "ClientFirstName", "Email", "IdentityNum", "LastName", "Password", "Username" },
                values: new object[] { "555-555-5555", "Alice", "alice.johnson@example.com", "2468013579", "Johnson", "password3", "client3" });

            migrationBuilder.InsertData(
                table: "Client",
                columns: new[] { "ClientId", "CellNum", "ClientFirstName", "Email", "IdentityNum", "LastName", "Password", "Username" },
                values: new object[] { 5, "666-666-6666", "Bob", "bob.wilson@example.com", "1357924680", "Wilson", "password4", "client4" });

            migrationBuilder.UpdateData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 3,
                column: "ClientId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 4,
                column: "ClientId",
                value: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Client",
                keyColumn: "ClientId",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Client",
                keyColumn: "ClientId",
                keyValue: 2,
                columns: new[] { "CellNum", "ClientFirstName", "Email", "IdentityNum", "LastName", "Password", "Username" },
                values: new object[] { "987-654-3210", "Jane", "jane.smith@example.com", "0987654321", "Smith", "password2", "client2" });

            migrationBuilder.UpdateData(
                table: "Client",
                keyColumn: "ClientId",
                keyValue: 3,
                columns: new[] { "CellNum", "ClientFirstName", "Email", "IdentityNum", "LastName", "Password", "Username" },
                values: new object[] { "555-555-5555", "Alice", "alice.johnson@example.com", "2468013579", "Johnson", "password3", "client3" });

            migrationBuilder.UpdateData(
                table: "Client",
                keyColumn: "ClientId",
                keyValue: 4,
                columns: new[] { "CellNum", "ClientFirstName", "Email", "IdentityNum", "LastName", "Password", "Username" },
                values: new object[] { "666-666-6666", "Bob", "bob.wilson@example.com", "1357924680", "Wilson", "password4", "client4" });

            migrationBuilder.InsertData(
                table: "Client",
                columns: new[] { "ClientId", "CellNum", "ClientFirstName", "Email", "IdentityNum", "LastName", "Password", "Username" },
                values: new object[] { 1, "123-456-7890", "John", "john.doe@example.com", "1234567890", "Doe", "password1", "client1" });

            migrationBuilder.UpdateData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 3,
                column: "ClientId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 4,
                column: "ClientId",
                value: 4);
        }
    }
}
