using Microsoft.AspNetCore.Mvc;
using InventoryManagement.API.Data;
using InventoryManagement.API.Models;

namespace InventoryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public PurchasesController(InventoryDbContext context)
        {
            _context = context;
        }

        // POST: api/Purchases
        [HttpPost]
        public async Task<ActionResult> PostPurchase([FromBody] PurchaseRequest request)
        {
            if (request == null || request.productId <= 0 || request.quantity <= 0)
                return BadRequest("Invalid purchase request");

            var inventory = await _context.Inventories.FindAsync(request.productId);
            if (inventory == null)
                return NotFound("Product not found");

            if (inventory.StockQty < request.quantity)
                return BadRequest("Insufficient stock");

            inventory.StockQty -= request.quantity;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                updatedProduct = new
                {
                    id = inventory.InventoryID,
                    quantity = inventory.StockQty,
                    itemName = inventory.ItemName,
                    category = inventory.Category,
                    price = inventory.Price
                }
            });
        }
    }

    public class PurchaseRequest
    {
        public int productId { get; set; }
        public int quantity { get; set; }
    }
}