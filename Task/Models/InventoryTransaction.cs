using System.ComponentModel.DataAnnotations;

namespace Task_Interview.Models
{
    public class InventoryTransaction
    {

        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product product { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string PerformedBy { get; set; } = string.Empty;
        public string SourceWarehouse { get; set; } =string.Empty;
        public string TargetWarehouse { get; set; } = string.Empty;
    }
}
