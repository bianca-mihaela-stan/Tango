using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TangoApp.Models
{
    public class MessageRecipient
    {
        [Key]
        public int MessageRecipientId { get; set; }

        public bool IsRead { get; set; }
        
        public string  UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int MessageId { get; set; }
        public virtual Message Message { get; set; }

        public int GroupMemberId { get; set; }
        public virtual GroupMember GroupMember { get; set; }
    }
}