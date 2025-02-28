using System.ComponentModel.DataAnnotations;

namespace moment5.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        public List<Ad>? Ads { get; set; }
    }
}