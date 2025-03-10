using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // Behövs för IFormFile

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

        [Display(Name = "Status")]
        public bool Status { get; set; } = true; // Standard: Annons är aktiv

        [Display(Name = "Skapad av")]
        public string? CreatedBy { get; set; } // Användare som skapade annonsen

        public int? CategoryId { get; set; }
        public Category? category { get; set; }

        // 🔹 Relation till bilder
        public virtual List<AdImage> Images { get; set; } = new();

        // 🔹 För att ta emot uppladdade bilder (icke-mappad till DB)
        [NotMapped]
        public List<IFormFile>? ImageFiles { get; set; }

        [Display(Name = "Såld till")]
        public string? Buyer { get; set; }
    }
}
