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
    public class MessagesController : Controller
    {
        // GET: Messages
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Edit(int id)
        {
            Message com = db.Messages.Find(id);
            if(!(User.IsInRole("Admin") || com.UserId == User.Identity.GetUserId()))
            {
                TempData["message"] = "Nu aveti dreptul sa editati aceasta postare!";
                return RedirectToAction("Show", "Groups", new { id = com.GroupId });
            }
            return View(com);

        }
        [HttpPut]
        public ActionResult Edit(int id, Message rcom)
        {

            try
            {
                if (ModelState.IsValid)
                {

                    Message com = db.Messages.Find(id);
                    if (User.IsInRole("Admin") || com.UserId == User.Identity.GetUserId())
                    {
                        if (TryUpdateModel(com))
                        {
                            ///stabilim modul de redirectionare
                            var conversationId = com.GroupId;
                            var currentUser = User.Identity.GetUserId();
                            if (com.Group.Status == GroupStatusFlag.PrivateConversation && User.IsInRole("Admin"))
                            {
                                TempData["message"] = "Mesajele din conversatiile private nu pot fi sterse de admin!";
                                return RedirectToAction("Index", "Groups");
                            }
                            else if(com.Group.Status == GroupStatusFlag.PrivateConversation)
                            {
                                conversationId = db.GroupMembers.Where(a => a.GroupId == com.GroupId && a.UserId == currentUser).ToList().First().GroupMemberId;

                            }
                            //facem schibarea efectiva
                            com.Content = rcom.Content;
                            com.LastEditDate = DateTime.Now;
                            TempData["message"] = "Comentariul a fost editat!";

                             //o sa trimitem notificari doar daca conversatie e de tipul groupmessages
                            if (User.IsInRole("Admin") && currentUser != com.UserId)
                            {
                                Notification not = new Notification();
                                not.GroupId = com.GroupId;
                                not.Type = NotificationFlag.EditedMessage;
                                not.Time = DateTime.Now;
                                not.MessageId = com.MessageId;
                                not.UserSendId = currentUser;
                                not.UserReceiveId = com.UserId;
                                db.Notifications.Add(not);
                            }
                            db.SaveChanges();
                            //redirectionam
                            var status = db.Groups.Find(com.GroupId).Status;
                            if ( status == GroupStatusFlag.PrivateConversation)
                                return RedirectToAction("ShowPrivateConv", "Groups", new { id = conversationId });
                            else
                                return RedirectToAction("Show", "Groups", new { id = conversationId });
                        }
                        else
                        {
                            return View(rcom);
                        }
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa editati acest mesaj!";
                        return RedirectToAction("Show", "Groups", new { id = com.GroupId });
                    }

                }
                else
                {
                    return View(rcom);
                }


            }
            catch (Exception e)
            {
                return View(rcom);
            }

        }
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Message com = db.Messages.Find(id);
            if(com == null)
            {
                TempData["messsage"] = "Mesajul acesta nu exista!";
                return RedirectToAction("Index", "Groups");
            }
            var currentUser = User.Identity.GetUserId();
            if (currentUser == com.UserId || User.IsInRole("Admin"))
            {
                var group = db.Groups.Find(com.GroupId);
                var conversationId = group.GroupId; ;

                if (group.Status == GroupStatusFlag.PrivateConversation)
                {
                    conversationId = db.GroupMembers.Where(a => a.GroupId == com.GroupId && a.UserId == currentUser).ToList().First().GroupMemberId;
                }
                else
                {
                    conversationId = group.GroupId;
                }
              
                if(User.IsInRole("Admin") && currentUser!=com.UserId)
                {
                    Notification not = new Notification();
                    not.GroupId = conversationId;
                    not.Type = NotificationFlag.DeletedMessage;
                    not.Time = DateTime.Now;
                    not.UserSendId = currentUser;
                    not.UserReceiveId = com.UserId;
                    db.Notifications.Add(not);
                    
                }
                var nots = db.Notifications.Where(u => u.MessageId == id).ToList();
                foreach (var notification in nots)
                {
                    db.Notifications.Remove(notification);

                }
                db.Messages.Remove(com);
                db.SaveChanges();
                TempData["message"] = "Comentariul a fost sters!";
                if (group.Status == GroupStatusFlag.PrivateConversation)
                    return RedirectToAction("ShowPrivateConv", "Groups", new { id = conversationId });
                else
                    return RedirectToAction("Show", "Groups", new { id = conversationId });
            }
            else
            {
                TempData["message"] = "Nu ai dreptul sa stergi acest mesaj!";
                return RedirectToAction("Show", "Groups", new { id = com.GroupId });
            }
        }
    }
}
