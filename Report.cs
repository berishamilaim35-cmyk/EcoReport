using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoReport.Models
{
    public class Report
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Titulli është i detyrueshëm")]
        [StringLength(200, ErrorMessage = "Titulli nuk mund të jetë më i gjatë se 200 karaktere")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Qyteti është i detyrueshëm")]
        [StringLength(100, ErrorMessage = "Qyteti nuk mund të jetë më i gjatë se 100 karaktere")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lokacioni është i detyrueshëm")]
        [StringLength(200, ErrorMessage = "Lokacioni nuk mund të jetë më i gjatë se 200 karaktere")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lloji i mbeturinave është i detyrueshëm")]
        [StringLength(50, ErrorMessage = "Lloji i mbeturinave nuk mund të jetë më i gjatë se 50 karaktere")]
        public string WasteType { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime DateReported { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? UploadedBy { get; set; } = string.Empty;

        //Kjo mushet ne backend o vorr sbon me bo required kto
        public string? UserId { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";
        [Column("Created")]
        public DateTime? Created { get; set; }
    }
}