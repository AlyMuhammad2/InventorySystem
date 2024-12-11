using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Interview.Data;
using Task_Interview.DTOs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Task_Interview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockReport(
                    [FromQuery] int currentPage = 1, [FromQuery] int pageSize = 3)   
        {
            if (currentPage < 1 || pageSize < 1)
            {
                return BadRequest("CurrentPage and PageSize must be greater than 0.");
            }

            var query = _context.Products
                .Include(p => p.ProductWarehouses)
                .ThenInclude(pw => pw.Warehouse)
                .Where(p => p.ProductWarehouses.Any(pw => pw.Quantity <= p.LowStockThreshold) && !p.IsDeleted);

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (currentPage > totalPages && totalPages > 0)
            {
                return NotFound("No data available for the requested page.");
            }

            var lowStockProducts = await query
                .OrderBy(p => p.Name) 
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize) 
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    LowStockThreshold = p.LowStockThreshold,
                    Price = p.Price,
                    Category = p.Category,
                    TotalQuantity = p.ProductWarehouses.Sum(pw => pw.Quantity),
                    Warehouses = p.ProductWarehouses
                        .Where(pw => pw.Quantity < p.LowStockThreshold)
                        .Select(pw => new WarehouseDto
                        {
                            Name = pw.Warehouse.Name,
                            Quantity = pw.Quantity
                        }).ToList()
                })
                .ToListAsync();

            string? nextPage = currentPage < totalPages
                             ? Url.Action("GetLowStockReport", new { currentPage = currentPage + 1, pageSize })
                             : null;

            string? previousPage = currentPage > 1
                                 ? Url.Action("GetLowStockReport", new { currentPage = currentPage - 1, pageSize })
                                 : null;

            var response = new
            {
                Data = lowStockProducts,
                TotalItems = totalItems,
                TotalPages = totalPages,
                NextPage = nextPage,
                PreviousPage = previousPage,
              
            };

            return Ok(response);
        }


        [HttpGet("transaction-history")]
        public async Task<IActionResult> GetTransactionHistory(int? productId,
                                           [FromQuery] DateTime? startDate,
                                           [FromQuery] DateTime? endDate,
                                           [FromQuery] string? transactionType,
                                           [FromQuery] string? category ,
                                           [FromQuery] int currentPage = 1, 
                                           [FromQuery] int pageSize = 3
                                           )
        {
            if (currentPage < 1 || pageSize < 1)
            {
                return BadRequest("CurrentPage and PageSize must be greater than 0." );
            }
            var query = _context.InventoryTransactions
                                        .Include(t => t.product) 
                                        .AsQueryable();

            if (productId.HasValue)
                query = query.Where(t => t.ProductId == productId.Value);
            

            if (startDate.HasValue && endDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value && t.Date <= endDate.Value);
            

            if (!string.IsNullOrEmpty(transactionType))
                query = query.Where(t => t.TransactionType != null && t.TransactionType.ToLower()==transactionType.ToLower());
            

            if (!string.IsNullOrEmpty(category))
                query = query.Where(t => t.product.Category != null && t.product.Category.ToLower()==category.ToLower());

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (currentPage > totalPages && totalPages > 0)
            {
                return NotFound("No data available for the requested page.");
            }
            var transactions = await query.OrderBy(t => t.Date).Skip((currentPage - 1) * pageSize).Take(pageSize)
                .Select(t => new
                {
                    TransactionId = t.Id,
                    ProductName = t.product.Name,
                    TransactionType = t.TransactionType,
                    Quantity = t.Quantity,
                    Date = t.Date,
                    Category = t.product.Category,
                    PerformedBy=t.PerformedBy
                })
                .ToListAsync();
            string? nextPage = currentPage < totalPages
                                ? Url.Action("GetTransactionHistory", new { productId, startDate, endDate, transactionType, category, currentPage = currentPage + 1, pageSize })
                                : null;
            string? previousPage = currentPage > 1
                                ? Url.Action("GetTransactionHistory", new { productId, startDate, endDate, transactionType, category, currentPage = currentPage - 1, pageSize })
                                : null;
            if (!transactions.Any())
                return NotFound( "No transactions found based on the provided filters." );

            var response = new
            {
                Data = transactions,
                TotalItems=totalItems,
                TotalPages=totalPages,
                NextPage = nextPage,
                PreviousPage = previousPage,
            };
            return Ok(response);
        }

    }
}
