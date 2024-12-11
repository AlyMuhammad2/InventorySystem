using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Interview.Data;
using Task_Interview.DTOs;
using Task_Interview.Models;

namespace Task_Interview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _context.Products
                 .Include(p => p.ProductWarehouses)
                 .ThenInclude(pw => pw.Warehouse) 
                 .Where(p => p.Id == id && !p.IsDeleted)
                 .Select(p => new ProductDto
                 {
                      Id = p.Id,
                      Name = p.Name,
                      Description = p.Description,
                      Price = p.Price,
                      LowStockThreshold = p.LowStockThreshold,
                      Category = p.Category,
                      TotalQuantity = p.ProductWarehouses.Sum(pw => pw.Quantity),
                      Warehouses = p.ProductWarehouses.Select(pw => new WarehouseDto
                      {
                              Name = pw.Warehouse.Name,
                              Quantity = pw.Quantity
                      }).ToList()
                 })
                 .FirstOrDefaultAsync();

                if (product == null)
                {
                return NotFound(new { Message = "Product not found." });
                }

              return Ok(product);
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListProducts()
        {
            var products = await _context.Products
                 .Include(p => p.ProductWarehouses) 
                 .ThenInclude(pw => pw.Warehouse)
                 .Where(p => !p.IsDeleted)  
                 .Select(p => new ProductDto
                 {
                     Id = p.Id,
                     Name = p.Name,
                     Description = p.Description,
                     Price = p.Price,
                     LowStockThreshold = p.LowStockThreshold,
                     Category = p.Category,
                     TotalQuantity = p.ProductWarehouses.Sum(pw => pw.Quantity),
                     Warehouses = p.ProductWarehouses.Select(pw => new WarehouseDto
                     {
                         Name = pw.Warehouse.Name,
                         Quantity = pw.Quantity
                     }).ToList()
                 })
                 .ToListAsync();


            return Ok(products);
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromBody] AddProductDto addProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Name == addProductDto.WarehouseName);
            if (warehouse == null)
            {
                return NotFound($"Warehouse '{addProductDto.WarehouseName}' not found.");
            }

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Name == addProductDto.Name && !p.IsDeleted);
            if (existingProduct != null)
            {
                return BadRequest("Product with the same name already exists.");
            }

            var product = new Product
            {
                Name = addProductDto.Name,
                Description = addProductDto.Description,
                Price = addProductDto.Price,
                LowStockThreshold = addProductDto.LowStockThreshold,
                Category = addProductDto.Category,
                IsDeleted = false
            };

            var productWarehouse = new ProductWarehouse
            {
                Product = product,
                WarehouseId = warehouse.Id,
                Quantity = addProductDto.Quantity
            };

            _context.Products.Add(product);  
            _context.ProductWarehouses.Add(productWarehouse); 
            await _context.SaveChangesAsync();

            return Ok("Product added successfully.");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || product.IsDeleted)
            {
                return NotFound("Product not found");
            }

            product.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok("Product deleted successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] AddProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var product = await _context.Products
                  .Include(p => p.ProductWarehouses)
                  .ThenInclude(pw => pw.Warehouse)
                  .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || product.IsDeleted)
            {
                return NotFound("Product not found");
            }
            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.LowStockThreshold = productDto.LowStockThreshold;
            product.Category = productDto.Category;
            if (!string.IsNullOrEmpty(productDto.WarehouseName))
            {
                var warehouse = await _context.Warehouses
                    .FirstOrDefaultAsync(w => w.Name == productDto.WarehouseName);

                if (warehouse == null)
                {
                    return NotFound($"Warehouse '{productDto.WarehouseName}' not found.");
                }

                var productWarehouse = product.ProductWarehouses
                    .FirstOrDefault(pw => pw.WarehouseId == warehouse.Id);

                if (productWarehouse == null)
                {
                    productWarehouse = new ProductWarehouse
                    {
                        ProductId = product.Id,
                        WarehouseId = warehouse.Id,
                        Quantity = productDto.Quantity
                    };
                    _context.ProductWarehouses.Add(productWarehouse);
                }
                else
                {
                    productWarehouse.Quantity = productDto.Quantity;
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Product updated successfully.");
        }
    }
}
