﻿using System.ComponentModel.DataAnnotations;

namespace Task_Interview.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
