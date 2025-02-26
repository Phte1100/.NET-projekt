using System.ComponentModel.DataAnnotations;

namespace moment5.Models
{
    public class Ad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        [Required]
        [StringLength(500)]
        public required string Description { get; set; }

        [Required]
        public required int Price { get; set; }

        public string? ImageUrl { get; set; } // URL till bild

        public bool status { get; set; } = true; // Om annonsen är aktiv eller inte

        public string? CreatedBy { get; set; } // Användare som skapade annonsen
    }
}