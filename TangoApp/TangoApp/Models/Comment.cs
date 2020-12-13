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
        public int CommentId { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Comentariul nu poate sa fie mai lung de 200 de caractere")]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
        public DateTime Date { get; set; }
        //adaugam si id-ul userului care lasa comentariul
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int PostId { get; set; }
        public virtual Post Post { get; set; }

    }
}