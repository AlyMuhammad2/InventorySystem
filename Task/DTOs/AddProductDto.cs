using System.ComponentModel.DataAnnotations;

namespace Task_Interview.DTOs
{
    public class AddProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int LowStockThreshold { get; set; }
        public string Category { get; set; }
        public string WarehouseName { get; set; }   
        public int Quantity { get; set; }
    }
}
