using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    [Authorize(Roles = "User,Editor,Admin")]
    public class CommentsController : Controller
    {
        // GET: Comments
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Edit(int id)
        {
            Comment com = db.Comments.Find(id);
            return View(com);
    
        }
        [HttpPut]
        public ActionResult Edit(int id, Comment rcom)
        {

            try
            {
                if(ModelState.IsValid)
                {

                    Comment com = db.Comments.Find(id);
                    if(TryUpdateModel(com))
                    {

                        com.Text = rcom.Text;
                        db.SaveChanges();
                        TempData["message"] = "Comentariul a fost editat!";
                        return Redirect("/Posts/Show/" + com.PostId);
                    }
                    else
                    {
                        return View(rcom);
                    }
         
                }
                else
                {
                    return View(rcom);
                }

       
            }
            catch(Exception e)
            {

                return View(rcom);
            }

        }
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Comment com = db.Comments.Find(id);
            
            //daca avem o notificare care sa o referentieze
            var notificationList = db.Notifications.Where(u => u.CommentId == com.CommentId).ToList();
            if(notificationList.Any())
            {
                var notification = notificationList.First();
                notification.CommentId = null;
            }
            if (User.IsInRole("Admin"))
            {
                Notification notification = new Notification
                {
                    UserSendId = User.Identity.GetUserId(),
                    UserReceiveId = com.UserId,
                    PostId = com.PostId,
                    Time = DateTime.Now,
                    Seen = false,
                    Type = NotificationFlag.DeletedComment
                };
                db.Notifications.Add(notification);
            }
            db.Comments.Remove(com);
            db.SaveChanges();
            TempData["message"] = "Comentariul a fost sters!";
            return Redirect("/Posts/Show/" + com.PostId);
        }
    }
}