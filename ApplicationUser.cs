using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoReport.Models
{
    public class ApplicationUser : IdentityUser
    {
    
        public ICollection<Report>? Reports { get; set; }
    }
}