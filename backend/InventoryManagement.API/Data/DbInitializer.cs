using InventoryManagement.API.Data;
using InventoryManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(InventoryDbContext context)
        {
            context.Database.Migrate();

            // Seed Users if none exist
            if (!context.Users.Any())
            {
                var users = new User[]
                {
                    new User { UserName = "Admin User", Email = "admin@example.com", UserType = "Admin", IsActive = true, CreatedDate = DateTime.Now },
                    new User { UserName = "John Doe", Email = "john@example.com", UserType = "User", IsActive = true, CreatedDate = DateTime.Now },
                    new User { UserName = "Jane Smith", Email = "jane@example.com", UserType = "Supplier", IsActive = true, CreatedDate = DateTime.Now }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // Seed Inventories if none exist
            if (!context.Inventories.Any())
            {
                var inventories = new Inventory[]
                {
                    new Inventory { ItemName = "Laptop", StockQty = 50, ReorderQty = 10, PriorityStatus = 0, Category = "Electronics", Price = 999.99m, Description = "High-performance laptop", SellerId = 1, CreatedDate = DateTime.Now },
                    new Inventory { ItemName = "Mouse", StockQty = 100, ReorderQty = 20, PriorityStatus = 0, Category = "Accessories", Price = 25.00m, Description = "Wireless mouse", SellerId = 2, CreatedDate = DateTime.Now },
                    new Inventory { ItemName = "Keyboard", StockQty = 30, ReorderQty = 15, PriorityStatus = 1, Category = "Accessories", Price = 50.00m, Description = "Mechanical keyboard", SellerId = 1, CreatedDate = DateTime.Now }
                };
                context.Inventories.AddRange(inventories);
                context.SaveChanges();
            }

            // Seed Stocks if none exist (sample transactions)
            if (!context.Stocks.Any())
            {
                var stocks = new Stock[]
                {
                    new Stock { InventoryID = 1, Quantity = 10, TransactionType = "IN", Remarks = "Initial stock", UserID = 1, TransactionDate = DateTime.Now },
                    new Stock { InventoryID = 2, Quantity = 5, TransactionType = "OUT", Remarks = "Sale", UserID = 2, TransactionDate = DateTime.Now }
                };
                context.Stocks.AddRange(stocks);
                context.SaveChanges();
            }
        }
    }
}