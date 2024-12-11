using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Interview.Migrations
{
    /// <inheritdoc />
    public partial class pagingAndIndxing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_InventoryTransactions_ProductId",
                table: "InventoryTransactions",
                newName: "IX_InventoryTransaction_ProductId");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionType",
                table: "InventoryTransactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_Date",
                table: "InventoryTransactions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_ProductId_Date",
                table: "InventoryTransactions",
                columns: new[] { "ProductId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_ProductId_TransactionType",
                table: "InventoryTransactions",
                columns: new[] { "ProductId", "TransactionType" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_TransactionType",
                table: "InventoryTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_TransactionType_Date",
                table: "InventoryTransactions",
                columns: new[] { "TransactionType", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Product_Category",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransaction_Date",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransaction_ProductId_Date",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransaction_ProductId_TransactionType",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransaction_TransactionType",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransaction_TransactionType_Date",
                table: "InventoryTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryTransaction_ProductId",
                table: "InventoryTransactions",
                newName: "IX_InventoryTransactions_ProductId");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionType",
                table: "InventoryTransactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
