using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TangoApp.Models;

namespace TangoApp.Models
{
    public class Comment
    {
        [Key]
        public int Commentid { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime Date { get; set; }
     
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

    }
}