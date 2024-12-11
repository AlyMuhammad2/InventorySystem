using Microsoft.EntityFrameworkCore;
using Task_Interview.Data;
using Task_Interview.Models;

namespace Task_Interview.Services
{
    public class StockTransferService
    {
        private readonly AppDbContext _context;

        public StockTransferService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Warehouse> GetWarehouseByName(string warehouseName)
        {
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Name == warehouseName);
            if (warehouse == null)
                throw new Exception($"Warehouse '{warehouseName}' not found.");
            return warehouse;
        }

        public async Task<ProductWarehouse> GetProductInWarehouse(int productId, int warehouseId)
        {
            var productWarehouse = await _context.ProductWarehouses
                    .Include(pw => pw.Product) 
                    .Include(pw => pw.Warehouse) 
                    .FirstOrDefaultAsync(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId);
            if (productWarehouse == null)
                throw new Exception("Product not found in the specified warehouse.");
            return productWarehouse;
        }

        public async Task TransferStockAsync(int productId, int quantity, string sourceWarehouseName, string targetWarehouseName, string performedBy)
        {
            var sourceWarehouse = await _context.Warehouses
                .Include(w => w.ProductWarehouses)
                .FirstOrDefaultAsync(w => w.Name == sourceWarehouseName);

            var targetWarehouse = await _context.Warehouses
                .Include(w => w.ProductWarehouses)
                .FirstOrDefaultAsync(w => w.Name == targetWarehouseName);

            if (sourceWarehouse == null || targetWarehouse == null)
            {
                throw new Exception("Source or target warehouse not found.");
            }

            var sourceProductWarehouse = sourceWarehouse.ProductWarehouses
                .FirstOrDefault(pw => pw.ProductId == productId);

            if (sourceProductWarehouse == null || sourceProductWarehouse.Quantity < quantity)
            {
                throw new Exception("Insufficient stock in source warehouse.");
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

            await _context.SaveChangesAsync();
        }
    }

}
