using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TangoApp.Models
{
    public class GroupMember
    {
        [Key]
        public int GroupMemberId { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int GroupId { get; set; }
        public virtual Group Group { get; set; }

        public MemberStatusFlag Status { get; set; }
    }
    public enum MemberStatusFlag
    {
       Member,
       Admin,
       Pending
    };
}