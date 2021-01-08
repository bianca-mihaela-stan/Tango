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
            if(User.IsInRole("Admin") || com.UserId == User.Identity.GetUserId())
            {
                return View(com);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati acest mesaj!";
                return RedirectToAction("Show", "Posts", new { id = com.PostId });
            }
          
    
        }
        [HttpPut]
        public ActionResult Edit(int id, Comment rcom)
        {

            try
            {
                if(ModelState.IsValid)
                {

                    Comment com = db.Comments.Find(id);
                    if (User.IsInRole("Admin") || com.UserId == User.Identity.GetUserId())
                    {
                        if (TryUpdateModel(com))
                        {

                            com.Text = rcom.Text;
                            if (User.IsInRole("Admin") && User.Identity.GetUserId() != com.UserId)
                            {
                                Notification notification = new Notification
                                {
                                    UserSendId = User.Identity.GetUserId(),
                                    UserReceiveId = com.UserId,
                                    PostId = com.PostId,
                                    Time = DateTime.Now,
                                    Seen = false,
                                    Type = NotificationFlag.EditedComment,
                                    CommentId  = com.CommentId
                                };
                                db.Notifications.Add(notification);
                            }
                            db.SaveChanges();
                            TempData["message"] = "Comentariul a fost editat!";
                            return Redirect("/Posts/Show/" + com.PostId);
                        }
                        else
                        {
                            TempData["message"] = "Something went wrong!";
                            return View(rcom);
                        }
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa editati acest mesaj!";
                        return RedirectToAction("Show", "Posts", new { id = com.PostId });
                    }
         
                }
                else
                {
                    TempData["message"] = "Something went wrong!";
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
            if (User.IsInRole("Admin") || User.Identity.GetUserId() == com.UserId)
            { 
                //daca avem o notificare care sa o referentieze
                var postid = com.PostId;
                var notificationList = db.Notifications.Where(u => u.CommentId == com.CommentId).ToList();
                if (notificationList.Any())
                {
                    foreach (var comm in notificationList)
                    {
                        db.Notifications.Remove(comm);
                    }
                }
                if (User.IsInRole("Admin") && User.Identity.GetUserId() != com.UserId)
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
                return Redirect("/Posts/Show/" + postid);

            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti aceast comentariu!";
                return Redirect("/Posts/Show/" + com.PostId);
            }
        }
    }
}