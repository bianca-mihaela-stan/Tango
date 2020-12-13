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
        public ActionResult Show(int id)
        {
            var notification = db.Notifications.Find(id);
            notification.Seen = true;
            //trebuie sa aflam tipul notificarii pentru a redirectiona user-ul
            //la o actiune dintr-un controller specific
            return Redirect("/Posts/Show/" + notification.PostId);
            
        }
        [NonAction]
        public void New(Notification notification)
        {
            notification.Time = DateTime.Now;
            notification.Seen = false;
            db.Notifications.Add(notification);
            db.SaveChanges();
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