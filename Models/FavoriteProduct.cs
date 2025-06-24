using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test_2.Models
{
    [Table("FavoriteProducts")]
    public class FavoriteProduct
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual User? User { get; set; }

        public virtual Product? Product { get; set; }
    }
}
