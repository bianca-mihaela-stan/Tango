using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Tango.Models
{
    public class Media
    {
        [Key]
        public int MediaId { get; set; }
        public string MediaPath { get; set; }
        public string MediaName { get; set; }
        public string MediaExtension { get; set; }

        public virtual Post Post { get; set; }
    }
}