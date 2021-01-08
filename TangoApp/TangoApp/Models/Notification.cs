using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace TangoApp.Models
{
    public class Notification
    {    
        //identificam notificarea pe baza unui int unic
        [Key]
        public int Id { get; set; }
       
        public DateTime Time { get; set; }

        public bool Seen { get; set; }
    
        public string UserReceiveId { get; set; }
        [ForeignKey("UserReceiveId")]
        public virtual ApplicationUser UserReceive { get; set; }

   
        public string UserSendId { get; set; }
        [ForeignKey("UserSendId")]
        public virtual ApplicationUser UserSend { get; set; }
   
        public int? PostId { get; set; }
        public virtual Post Post { get; set; }
    
        public int? CommentId { get; set; }
        public virtual Comment Comment { get; set; }
        public int? GroupId { get; set; }
        public virtual Group Group { get; set; }

        public int? MessageId { get; set; }
        public virtual Message Message { get; set; }

        public NotificationFlag Type { get; set; }
    }
    public enum NotificationFlag
    {
        DeletedComment,
        DeletedPost,
        NewComment,
        NewPost,
        DeletedMedia,
        JoinRequest,
        AskToJoinRequest,
        NewMember,
        RemovedFromGroup,
        AddedToGroup,
        MadeAdmin,
        DeletedMessage,
        EditedMessage,
        EditedComment,
        EditedPost
    };
}