namespace Task_Interview.DTOs
{
    public class ProductListDto
    {
        public int Id { get; set; }  
        public string Name { get; set; }  
        public string Description { get; set; }  
        public decimal Price { get; set; } 
        public string Category { get; set; } 
        public int LowStockThreshold { get; set; } 
        public int TotalQuantity { get; set; }
    }
}
