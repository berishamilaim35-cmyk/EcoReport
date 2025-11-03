using System.ComponentModel.DataAnnotations;

namespace EcoReport.Models
{
    public class LeaderBoardEntry
    {
        [Key] 
        public int Id { get; set; }
        public string UserName { get; set; }
        public int TotalReports { get; set; }
        public int Points { get; set; }
    }
}
