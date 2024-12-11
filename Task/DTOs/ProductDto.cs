namespace Task_Interview.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal Price { get; set; }
        public int LowStockThreshold { get; set; }
        public string? Category { get; set; } = string.Empty;
        public List<WarehouseDto> Warehouses { get; set; } = new List<WarehouseDto>();  

    }

}