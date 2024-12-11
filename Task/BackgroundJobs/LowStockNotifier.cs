using Task_Interview.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Task_Interview.BackgroundJobs
{
    public class LowStockNotifier
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LowStockNotifier> _logger;

        public LowStockNotifier(AppDbContext context, ILogger<LowStockNotifier> logger)
        {
            _context = context;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task NotifyLowStockItems()
        {
            var lowStockProducts = await _context.ProductWarehouses
                .Include(pw => pw.Product)
                .Include(pw => pw.Warehouse)
                .Where(pw => pw.Quantity <= pw.Product.LowStockThreshold)
                .ToListAsync();
            if (!lowStockProducts.Any())
            {
                _logger.LogInformation(" No low stock products found.");
                return;
            }

            foreach (var product in lowStockProducts)
            {
                _logger.LogWarning($"Low stock alert: Product '{product.Product.Name}' in Warehouse '{product.Warehouse.Name}' has only {product.Quantity} left (Threshold: {product.Product.LowStockThreshold}).");
            }
        }
    }
}
