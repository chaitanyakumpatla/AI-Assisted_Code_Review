using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Data;
using InventoryManagement.API.Models;

namespace InventoryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public InventoryController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventories(int? sellerId = null)
        {
            var query = _context.Inventories.AsQueryable();
            if (sellerId.HasValue)
            {
                query = query.Where(i => i.SellerId == sellerId.Value);
            }
            return await query.ToListAsync();
        }

        // GET: api/Inventory/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory>> GetInventory(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return NotFound();
            return inventory;
        }

        // POST: api/Inventory
        // Accept a flexible request from the frontend (may include seller-specific fields)
        [HttpPost]
        public async Task<ActionResult<Inventory>> PostInventory([FromBody] ProductCreateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProductName))
                return BadRequest("Invalid product data. 'productName' is required.");

            // Map request to Inventory entity. Frontend sends product-oriented JSON (productName, quantity, etc.).
            var inventory = new Inventory
            {
                ItemName = request.ProductName,
                StockQty = request.Quantity ?? 0,
                Category = request.Category,
                Price = request.Price,
                Description = request.ProductDescription,
                SellerId = request.SellerId ?? 0,
                // Set defaults for required fields.
                ReorderQty = 0,
                PriorityStatus = 0,
                CreatedDate = DateTime.Now,
                LastUpdated = null
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInventory", new { id = inventory.InventoryID }, inventory);
        }

        // PUT: api/Inventory/5
        // Use a safer update approach: load the existing entity, apply only intended changes,
        // then save. This prevents accidental overwrites when the frontend sends partial data
        // (e.g., only updating the SellerId) and ensures EF is tracking the entity.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventory(int id, InventoryUpdateRequest request)
        {
            var existing = await _context.Inventories.FindAsync(id);
            if (existing == null) return NotFound();

            // Apply incoming values only if they are provided (not null).
            if (request.ItemName != null) existing.ItemName = request.ItemName;
            if (request.StockQty.HasValue) existing.StockQty = request.StockQty.Value;
            if (request.ReorderQty.HasValue) existing.ReorderQty = request.ReorderQty.Value;
            if (request.PriorityStatus.HasValue) existing.PriorityStatus = request.PriorityStatus.Value;
            if (request.Category != null) existing.Category = request.Category;
            if (request.Price.HasValue) existing.Price = request.Price.Value;
            if (request.Description != null) existing.Description = request.Description;
            if (request.SellerId.HasValue) existing.SellerId = request.SellerId.Value;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Inventory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return NotFound();

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Inventory/UpdateStock
        [HttpPost("UpdateStock")]
        public async Task<ActionResult> UpdateStock([FromBody] StockUpdateRequest request)
        {
            var inventory = await _context.Inventories.FindAsync(request.InventoryID);
            if (inventory == null) return NotFound();

            // Update inventory quantity
            if (request.TransactionType == "IN")
                inventory.StockQty += request.Quantity;
            else if (request.TransactionType == "OUT")
                inventory.StockQty -= request.Quantity;
            else
                inventory.StockQty = request.Quantity; // ADJUSTMENT

            // Auto-update PriorityStatus based on stock level
            inventory.PriorityStatus = inventory.StockQty <= inventory.ReorderQty ? 1 : 0; // 1 = High if low stock

            // Auto-calculate ReorderQty if stock is low after OUT transaction
            if (request.TransactionType == "OUT" && inventory.StockQty <= 10)
            {
                inventory.ReorderQty = 20 - inventory.StockQty; // Example: reorder enough to reach 20
            }

            // LastUpdated is now auto-set in DbContext

            // Create stock transaction record
            var stockTransaction = new Stock
            {
                InventoryID = request.InventoryID,
                Quantity = request.Quantity,
                TransactionType = request.TransactionType,
                Remarks = request.Remarks,
                UserID = request.UserID,
                TransactionDate = DateTime.Now
            };

            _context.Stocks.Add(stockTransaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Stock updated successfully", newQuantity = inventory.StockQty, reorderQty = inventory.ReorderQty });
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventories.Any(e => e.InventoryID == id);
        }
    }

    public class StockUpdateRequest
    {
        public int InventoryID { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; } // IN, OUT, ADJUSTMENT
        public string Remarks { get; set; }
        public int UserID { get; set; }
    }

    // DTO used for creating a product from frontend. This allows the frontend to send
    // extra seller-related fields without causing model validation errors on the Inventory entity.
    public class ProductCreateRequest
    {
        // Match frontend JSON: productName, productDescription, category, price, quantity
        // Nullable so automatic model validation doesn't return 400 before controller logic runs.
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? Category { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public int? SellerId { get; set; }
    }

    public class InventoryUpdateRequest
    {
        public string? ItemName { get; set; }
        public int? StockQty { get; set; }
        public int? ReorderQty { get; set; }
        public int? PriorityStatus { get; set; }
        public string? Category { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public int? SellerId { get; set; }
    }
}
