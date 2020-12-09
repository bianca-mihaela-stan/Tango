using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Tango.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public Post PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}