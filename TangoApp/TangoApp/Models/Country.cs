using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TangoApp.Models
{
    public class Country
    {
        [Key]
        public int CoutryId { get; set; }
        public string CountryName { get; set; }

        public ICollection<City> Cities { get; set; }
    }
}