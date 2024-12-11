using System.ComponentModel.DataAnnotations;

namespace Task_Interview.Models
{
    public class ArchivedTransaction
    {
        [Key]
        public int Id { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int ProductId { get; set; } 
        public int Quantity { get; set; } 
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string PerformedBy { get; set; }
        public string SourceWarehouse { get; set; } = string.Empty;
        public string TargetWarehouse { get; set; } = string.Empty;
    }
}
