using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TangoApp.Models
{
    public class Profile
    {
        [Key]
        public int ProfileId { get; set; }
        public bool ProfileVisibility { get; set; }
        public string Description { get; set; }
        public bool? Gender { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? Birthday { get; set; }

        public int? CityId { get; set; }
        public virtual City City { get; set; }

        public int? CountryId {get; set;}
        public virtual Country Country { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public IEnumerable<SelectListItem> Countries { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }
    }
}