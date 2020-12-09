using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Tango.Models;

namespace Tango.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Required]
        [StringLength(200,ErrorMessage = "Comentariul nu poate sa fie mai lung de 200 de caractere")]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
        public DateTime Date { get; set; }
     
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

    }
}