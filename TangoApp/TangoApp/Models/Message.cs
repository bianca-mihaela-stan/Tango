using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TangoApp.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Continutul mesajului nu poate sa fie gol.")]
        [StringLength(10000, ErrorMessage = "Comentariul nu poate sa fie mai lung de 10000 de caractere")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
        public DateTime? LastEditDate { get; set; }

        public int? GroupId { get; set; }
        public virtual Group Group { get; set; }

        //adding UserId
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<MessageRecipient> MessageRecipients { get; set; }
    }
}