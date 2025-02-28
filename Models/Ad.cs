using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace moment5.Models
{
    public class Ad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Titel")]
        public required string Title { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Beskrivning")]
        public required string Description { get; set; }

        [Required]
        [Display(Name = "Pris")]
        public required int Price { get; set; }

        public string? ImageName { get; set; }

        [NotMapped]
        [Display(Name = "Bild")]
        public IFormFile? ImageFile { get; set; }

        public bool status { get; set; } = true; // Om annonsen är aktiv eller inte

        [Display(Name = "Skapad av")]
        public string? CreatedBy { get; set; } // Användare som skapade annonsen

        public int? CategoryId { get; set; }
        public Category? category { get; set; }
    }
}