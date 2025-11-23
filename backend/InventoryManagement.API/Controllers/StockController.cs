using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.API.Data;
using InventoryManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public StockController(InventoryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all stock transactions (includes Inventory navigation property).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
        {
            var stocks = await _context.Stocks
                .Include(s => s.Inventory)
                .OrderByDescending(s => s.TransactionDate)
                .ToListAsync();

            // Return Ok with the list (empty list is valid)
            return Ok(stocks);
        }

        /// <summary>
        /// Get a specific stock transaction by ID (includes Inventory navigation property).
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Stock>> GetStock(int id)
        {
            var stock = await _context.Stocks
                .Include(s => s.Inventory)
                .FirstOrDefaultAsync(s => s.StockID == id);

            if (stock == null)
            {
                return NotFound();
            }

            return stock;
        }

        /// <summary>
        /// Get all stock transactions for a specific user.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Stock>>> GetUserStocks(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var userStocks = await _context.Stocks
                .Include(s => s.Inventory)
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.TransactionDate)
                .ToListAsync();

            if (userStocks == null || !userStocks.Any())
            {
                return NotFound($"No stock transactions found for user ID {userId}.");
            }

            return Ok(userStocks);
        }
    }
}