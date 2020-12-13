using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TangoApp.Models
{
    public class Friendship
    {
        
        [Key]
        public int FriendshipId { get; set; }
        [Range(0,3)]
        public int Status { get; set; }

        //[Key, Column(Order = 0)]
        public string User1Id { get; set; }
        [ForeignKey("User1Id")]
        public virtual ApplicationUser User1 { get; set; }

        //[Key, Column(Order = 1)]
        public string User2Id { get; set; }
        [ForeignKey("User2Id")]
        public virtual ApplicationUser User2 { get; set; }

        public int ActionUser { get; set; }
        //[ForeignKey("ActionUserId")]
        //public virtual ApplicationUser ActionUser { get; set; }

    }
}