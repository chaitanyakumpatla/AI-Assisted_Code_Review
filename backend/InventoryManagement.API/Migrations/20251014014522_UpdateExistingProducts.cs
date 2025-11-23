using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update LastUpdated for existing records to CreatedDate
            migrationBuilder.Sql("UPDATE Inventories SET LastUpdated = CreatedDate WHERE LastUpdated IS NULL");

            // Update ReorderQty for existing records: if StockQty <= 10, set to 20 - StockQty, else 0
            migrationBuilder.Sql("UPDATE Inventories SET ReorderQty = CASE WHEN StockQty <= 10 THEN 20 - StockQty ELSE 0 END");

            // Update PriorityStatus for existing records: 1 if StockQty <= ReorderQty, else 0
            migrationBuilder.Sql("UPDATE Inventories SET PriorityStatus = CASE WHEN StockQty <= ReorderQty THEN 1 ELSE 0 END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
