﻿using System.ComponentModel.DataAnnotations;

namespace Pustok.Models
{
    public class BookReview
    {
        public int Id { get; set; }
        public string AppUserId { get; set; }
        public int BookId { get; set; }
        [Required]
        [Range(1,5)]
        public byte Rate { get; set; }
        [MaxLength(500)]
        public string Text { get;set; }
        public DateTime CreatedAt { get; set; }

        public Book Book { get; set; }  
        public AppUser AppUser { get; set; }
    }
}
