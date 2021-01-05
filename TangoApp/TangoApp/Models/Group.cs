using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TangoApp.Models
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        [StringLength(400, ErrorMessage = "Descriere nu poate sa fie mai lunga de 200 de caractere")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public GroupStatusFlag Status { get; set; }
    }
    public enum GroupStatusFlag
    {
        MessageGroup,
        PrivateConversation
    };
}