using System.ComponentModel.DataAnnotations;

namespace Task_Interview.Models
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Location { get; set; } = string.Empty;

        public ICollection<ProductWarehouse> ProductWarehouses { get; set; } = new List<ProductWarehouse>();
    }
}
