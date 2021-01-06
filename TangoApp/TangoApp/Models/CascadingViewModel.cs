using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace TangoApp.Models
{
    public class CascadingViewModel
    {
        [DisplayName("Country")]
        public int CountryId { get; set; }
        [DisplayName("City")]
        public int CityId { get; set; }
        public List<Country> Countries { get; set; }
        public List<City> Cities { get; set; }
    }
}