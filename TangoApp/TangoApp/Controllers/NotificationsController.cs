using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class NotificationsController : Controller
    {
        // GET: Notifications
        private ApplicationDbContext db = new ApplicationDbContext();
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Index()
        {
            var currentUserId = User.Identity.GetUserId();
            var notifications = db.Notifications.Where(x => x.UserReceiveId == currentUserId).OrderByDescending(x => x.Time).ToList();
            ViewBag.Notifications = notifications;
            return View();
        }
        public ActionResult ShowPost(int id)
        {
            var notification = db.Notifications.Find(id);
            notification.Seen = true;
            //trebuie sa aflam tipul notificarii pentru a redirectiona user-ul
            //la o actiune dintr-un controller specific
            return Redirect("/Posts/Show/" + notification.PostId);
            
        }
        public ActionResult ShowGroup(int id)
        {
            var notification = db.Notifications.Find(id);
            notification.Seen = true;
            return Redirect("/Groups/Show/" + notification.GroupId);
        }
        [NonAction]
        public ActionResult Collapse(int id)
        {
            return View();

        }
        [NonAction]
        public void Delete(int id)
        {
            Notification notification = db.Notifications.Find(id);
            db.Notifications.Remove(notification);
            db.SaveChanges();
        }

    }
}