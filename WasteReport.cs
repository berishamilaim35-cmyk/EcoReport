using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoReport.Models
{
    public class WasteReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
   
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; }

        [Required]
        [MaxLength(50)]
        public string WasteType { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        [MaxLength(100)]
        public string UploadedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
    }
}
