using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace moment5.Models
{
    public class AdImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImageName { get; set; } = string.Empty;

        // 🔹 Koppling till en annons
        public int AdId { get; set; }
        public virtual Ad? Ad { get; set; }
    }
}
