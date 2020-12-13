﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TangoApp.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }
        [Required(ErrorMessage ="Continutul postarii nu poate sa fie gol.")]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
        public DateTime Date {get; set;}
        public DateTime? LastEditDate { get; set; }
        //adaugam UserId-ul
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
    }

}