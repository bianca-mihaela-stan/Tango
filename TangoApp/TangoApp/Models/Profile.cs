using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TangoApp.Models
{
    public class Profile
    {
        [Key]
        public int ProfileId { get; set; }
        public bool ProfileVisibility { get; set; }
        public string Description { get; set; }
        public bool? Gender { get; set; }
        public DateTime? Birthday { get; set; }

        public int? CityId { get; set; }
        public City City { get; set; }

        public int? CountryId {get; set;}
        public Country Country { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}