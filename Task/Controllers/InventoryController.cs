using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task_Interview.Data;
using Task_Interview.Models;
using Task_Interview.Services;

namespace Task_Interview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly StockTransferService _stockTransferService;

        public InventoryController(AppDbContext context, StockTransferService stockTransferService)
        {
            _context = context;
            _stockTransferService = stockTransferService;
        }

        [HttpPost("add-stock")]
        public async Task<IActionResult> AddStock(int productId, string warehouseName, int quantity)
        {
            var performedBy = User.Identity?.Name;
            if (string.IsNullOrEmpty(performedBy))
            {
                return Unauthorized("User is not authenticated." );
            }

            var productWarehouse = await _context.ProductWarehouses
                .Include(pw => pw.Product)
                .Include(pw => pw.Warehouse)
                .FirstOrDefaultAsync(pw => pw.Product.Id == productId && pw.Warehouse.Name == warehouseName);

            if (productWarehouse == null)
            {
                return NotFound( "Product or Warehouse not found." );
            }

            productWarehouse.Quantity += quantity;

            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = productId,
                TransactionType = "Add",
                Quantity = quantity,
                PerformedBy = performedBy,
                SourceWarehouse = warehouseName
            });

            await _context.SaveChangesAsync();
            return Ok("Stock added successfully.");
        }

        [HttpPost("remove-stock")]
        public async Task<IActionResult> RemoveStock(int productId, string warehouseName, int quantity)
        {
            var performedBy = User.Identity?.Name;
            if (string.IsNullOrEmpty(performedBy))
            {
                return Unauthorized("User is not authenticated.");
            }
            var product = await _context.ProductWarehouses.Include(pw=>pw.Product)
                                                          .Include (pw => pw.Warehouse)
                                                          .FirstOrDefaultAsync(pw=>pw.ProductId==productId && pw.Warehouse.Name==warehouseName);

            if (product == null) return NotFound("Product not found");
            if (product.Quantity < quantity) return BadRequest("Insufficient stock");

            product.Quantity -= quantity;
            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = productId,
                TransactionType = "Remove",
                Quantity = quantity,
                PerformedBy = performedBy, 
                SourceWarehouse = warehouseName
            });

            await _context.SaveChangesAsync();
            return Ok("Stock removed successfully.");
        }

        [HttpPost("transfer-stock")]
        public async Task<IActionResult> TransferStock(int productId, int quantity,
                                                       string sourceWarehouseName,
                                                       string targetWarehouseName)
        {
            var performedBy = User.Identity?.Name;
            if (string.IsNullOrEmpty(performedBy))
            {
                return Unauthorized("User is not authenticated.");
            }
            var sourceWarehouse = await _context.Warehouses
                 .Include(w => w.ProductWarehouses)
                 .FirstOrDefaultAsync(w => w.Name == sourceWarehouseName);

            if (sourceWarehouse == null)
            {
                return NotFound($"Source warehouse '{sourceWarehouseName}' not found.");
            }

            var targetWarehouse = await _context.Warehouses
                .Include(w => w.ProductWarehouses)
                .FirstOrDefaultAsync(w => w.Name == targetWarehouseName);

            if (targetWarehouse == null)
            {
                return NotFound($"Target warehouse '{targetWarehouseName}' not found.");
            }

            var sourceProductWarehouse = sourceWarehouse.ProductWarehouses
                .FirstOrDefault(pw => pw.ProductId == productId);

            if (sourceProductWarehouse == null || sourceProductWarehouse.Quantity < quantity)
            {
                return BadRequest($"Insufficient stock in source warehouse '{sourceWarehouseName}' or product not found.");
            }

            var targetProductWarehouse = targetWarehouse.ProductWarehouses
                .FirstOrDefault(pw => pw.ProductId == productId);

            if (targetProductWarehouse == null)
            {
                targetProductWarehouse = new ProductWarehouse
                {
                    ProductId = productId,
                    WarehouseId = targetWarehouse.Id,
                    Quantity = 0
                };

                _context.ProductWarehouses.Add(targetProductWarehouse);
            }

            sourceProductWarehouse.Quantity -= quantity;
            targetProductWarehouse.Quantity += quantity;
            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = productId,
                TransactionType = "Transfer",
                SourceWarehouse=sourceWarehouseName,
                TargetWarehouse=targetWarehouseName,
                Quantity = quantity,
                PerformedBy = performedBy
            });
            await _context.SaveChangesAsync();

            return Ok($"Successfully transferred {quantity} units of product ID {productId} from '{sourceWarehouseName}' to '{targetWarehouseName}'.");
           
        }
      
    }
}
