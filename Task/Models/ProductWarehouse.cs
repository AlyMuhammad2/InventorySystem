﻿namespace Task_Interview.Models
{
    public class ProductWarehouse
    {

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int Quantity { get; set; }
    }
}
