using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class MediaController : Controller
    {
        // GET: Media
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpDelete]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Delete(int id)
        {
            var media = db.Media.Find(id);
            var postId = media.Post.PostId;
            if (media.Post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
               
               
                ///daca continutul a fost gasit necorespunzator si a fost sters de admin, atunci
                // trebuie sa adaugam o notificare corespunzatoare pentru User-ul care a postat postarea/imaginea
                if (User.IsInRole("Admin"))
                {
                    Notification notification = new Notification();
                    notification.UserSendId = User.Identity.GetUserId();
                    notification.UserReceiveId = media.Post.UserId;
                    notification.Time = DateTime.Now;
                    notification.Seen = false;
                    notification.PostId = media.PostId;
                    notification.Type = NotificationFlag.DeletedMedia;
                    db.Notifications.Add(notification);

                }
                TempData["message"] = "Poza a fost stearsa!";
                db.Media.Remove(media);
                db.SaveChanges();
                return Redirect("/Posts/Show/" + postId);
            }
            else
            {
                TempData["message"] = "Nu aveti permisiunea de a sterge aceasta poza!";
                return Redirect("/Posts/Show/" + postId);
            }
           
        }
    }
}