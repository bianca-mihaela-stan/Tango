using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Tango.Models;

namespace Tango.Models
{
    public class Like
    {
        [Key]
        public int LikeId { get; set; }
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

    }
}