using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CLDV6211_ST10287165_POE_P1.Migrations
{
    /// <inheritdoc />
    public partial class seededs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "Category", "ClientId", "Description", "ImageType", "ImageUrl", "InStock", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Handcrafted Decorative Collectibles", 1, "Native African Navajo Hand Carved Pottery Vase 3 1/2\"-Signed \"IN\" Navajo", "Url", "https://yerbamatehurt.com/eng_pl_Chilean-Yerba-Mate-Cup-COPIAPO-with-beautiful-Virolle-made-of-steel-4871_2.jpg", "in stock", "Navajo Etched Black Terracotta 3\" Pottery Vase Signed RONDA", 15795.00m },
                    { 2, "Decorative Collectibles", 2, "Made by Pomotso Mafura and painted by Nozipho Ntshalintshali\r\n\r\nDimensions: H 44cm; L 30cm; W 30cm", "Url", "https://www.ardmore-design.com/cdn/shop/products/NP339OCT21_2-626807.jpg?v=1695893700&width=823", "out of stock", "Tanzanite Leopard Vase", 51000.00m },
                    { 3, "Handcrafted Decorative Collectibles", 3, "African tribal terracotta clay vessel that displays an aged patina, general surface wear,and small scattered nicks.", "Url", "https://www.thewildcoasttradingcompany.co.za/cdn/shop/products/Yanela_African_Pot_2_1024x1024.jpg?v=1575627030", "in stock", "African Tribal Terracotta Clay Vessel Possible Water Pot 11.75\" High", 12000.00m },
                    { 4, "Handcrafted Decorative Collectibles", 4, "A SINGLE Figure Made of High Quality Poly Resin - Hand Painted Décor -Measurement: 15.5 inches tall.", "Url", "https://m.media-amazon.com/images/I/41lsqyheQcL._SL500_.jpg", "in stock", "African Figurine Sculpture Colorful Dress Sitting Down Lady Figurine Holding Vase", 5500.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
