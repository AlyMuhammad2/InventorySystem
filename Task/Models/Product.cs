using System.ComponentModel.DataAnnotations;

namespace Task_Interview.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalQuantity => ProductWarehouses.Sum(pw => pw.Quantity);
        public decimal Price { get; set; }
        public int LowStockThreshold { get; set; }
        public bool IsDeleted { get; set; } = false; // Soft del
        public string Category {  get; set; } =string.Empty;
        public ICollection<ProductWarehouse> ProductWarehouses { get; set; } = new List<ProductWarehouse>();
    }
}
