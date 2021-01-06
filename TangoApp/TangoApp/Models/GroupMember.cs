using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TangoApp.Models
{
    public class GroupMember
    {
        [Key]
        public int GroupMemberId { get; set; }
        [Index("GroupMemberUniIndex",1,IsUnique = true)]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        [Index("GroupMemberUniIndex", 2, IsUnique = true)]
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