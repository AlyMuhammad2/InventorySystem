using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Task_Interview.Data;
using Task_Interview.Models;

namespace Task_Interview.BackgroundJobs
{
    public class TransactionArchiver
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TransactionArchiver> _logger;

        public TransactionArchiver(AppDbContext context, ILogger<TransactionArchiver> logger)
        {
            _context = context;
            _logger = logger;

        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ArchiveOldTransactions()
        {
            var oneYearAgo = DateTime.UtcNow.AddYears(-1);

            var oldTransactions = await _context.InventoryTransactions
                .Where(t => t.Date <= oneYearAgo)
                .ToListAsync();
            if (!oldTransactions.Any())
            {
                _logger.LogInformation("No transactions to archive.");
                return;
            }
            var archivedTransactions = oldTransactions.Select(t => new ArchivedTransaction
            {
                TransactionType = t.TransactionType,
                ProductId = t.ProductId,
                Quantity = t.Quantity,
                Date = t.Date,
                SourceWarehouse=t.SourceWarehouse,
                TargetWarehouse=t.TargetWarehouse,
                PerformedBy = t.PerformedBy
            }).ToList();
            await _context.ArchivedTransactions.AddRangeAsync(archivedTransactions);
            _context.InventoryTransactions.RemoveRange(oldTransactions);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"{archivedTransactions.Count} transactions archived.");

        }
    }
}
