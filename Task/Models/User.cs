using System.ComponentModel.DataAnnotations;

namespace Task_Interview.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; 
        public int RoleId { get; set; } 
        public Role Role { get; set; } 
    }
    
}
