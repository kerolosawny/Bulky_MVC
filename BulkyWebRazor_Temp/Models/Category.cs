﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BulkyBookWebRazor_Temp.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Category Name")]
        [MaxLength(30)]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must me between 1 and 100")]
        public int DisplayOrder { get; set; }
    }
}
