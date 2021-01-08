using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
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
                    if (TryUpdateModel(com))
                    {

                        com.Content = rcom.Content;
                        db.SaveChanges();
                        TempData["message"] = "Comentariul a fost editat!";
                        var currentUser = User.Identity.GetUserId();
                        var conversationId = db.GroupMembers.First(a => a.GroupId == com.GroupId && a.UserId == currentUser).GroupMemberId;
                        if (db.Groups.Find(com.GroupId).Status == GroupStatusFlag.PrivateConversation)
                            return RedirectToAction("ShowPrivateConv", "Groups", new { id = conversationId });
                        else
                            return RedirectToAction("Show", "Groups", new { id = com.GroupId });
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
            catch (Exception e)
            {
                return View(rcom);
            }

        }
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Message com = db.Messages.Find(id);
            var currentUser = User.Identity.GetUserId();
            var group = db.Groups.First(a => a.GroupId == com.GroupId);
            var conversationId=  group.GroupId; ;
            if (group.Status == GroupStatusFlag.PrivateConversation)
            {
                conversationId = db.GroupMembers.First(a => a.GroupId == com.GroupId && a.UserId == currentUser).GroupMemberId;
            }
            else
            {
                conversationId = group.GroupId;
            }
            db.Messages.Remove(com);
            db.SaveChanges();
            TempData["message"] = "Comentariul a fost sters!";
            if (group.Status == GroupStatusFlag.PrivateConversation)
                return RedirectToAction("ShowPrivateConv", "Groups", new { id = conversationId });
            else
                return RedirectToAction("Show", "Groups", new { id = com.GroupId });
        }
    }
}
