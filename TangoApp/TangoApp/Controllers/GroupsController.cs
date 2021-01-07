using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class GroupsController : Controller
    {
        // GET: Groups
        private ApplicationDbContext db = new ApplicationDbContext();
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Index()
        {
            var groups = db.Groups.Where(u => u.Status == GroupStatusFlag.MessageGroup).ToList();
            if(TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            ViewBag.Groups = groups;
            return View();
        }
        public ActionResult IndexPrivateConv()
        {
            var currentUserId = User.Identity.GetUserId();
            //vreau sa iau id-urile tuturor conversatiilor private
            var conversations = db.GroupMembers.Where(u => u.UserId == currentUserId && u.Group.Status == GroupStatusFlag.PrivateConversation).ToList().Select(u => u.GroupId).Distinct();
            //vreau acum sa iau toate intrarile groupmember care contin prietenii utilizatorului curent
            ViewBag.Conversations = db.GroupMembers.Where(u => conversations.Contains(u.GroupId) && u.UserId != currentUserId).ToList();
            ViewBag.UtilizatorCurent = currentUserId;
            return View();

        }
        
        [HttpPost]
        public ActionResult ShowPrivateConv(FormCollection formData)
        {
            var currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.Find(currentUserId);
            var friendId = formData.Get("Userid");
            var friend = db.Users.Find(friendId);
            if(friend == null)
            {
                TempData["message"] = "Utilizatorul nu exista!";
                return RedirectToAction("Index", "Users");
            }
            //trebuie sa verificam ca cei 2 useri sa fie prieteni
            var arefriends = db.Friends.Where(u => (u.User1Id == friendId && u.User2Id == currentUserId) || (u.User2Id == friendId && u.User1Id == currentUserId));
            if(!arefriends.Any())
            {
                TempData["message"] = "Nu esti prieten cu acest utilizator. Nu poti sa ii scrii mesaj privat!";
                return RedirectToAction("Show", "Profile", new { id = friendId });
            }
            var conversationFriend = db.GroupMembers.Where(u => u.UserId == friendId && u.Group.Status == GroupStatusFlag.PrivateConversation).ToList().Select(u => u.Group);
            var conversationUserCurrent = db.GroupMembers.Where(u => u.UserId == currentUserId && u.Group.Status == GroupStatusFlag.PrivateConversation).ToList().Select(u => u.Group);
            var conversation = conversationFriend.Intersect(conversationUserCurrent);
            if (!conversation.Any() || conversation.Count() != 1)
            {
                TempData["message"] =  "Something went terribly wrong!";
                return RedirectToAction("Show", "Profile", new { id = friendId });
            }
            var groupconversation = conversation.First();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            ViewBag.UserCurrent = currentUser;
            ViewBag.UserFriend = friend;
            return View(groupconversation);

        }
        public ActionResult Show(int id)
        {
            var grup = db.Groups.Find(id);
            var currentUser = User.Identity.GetUserId();
            var invites = db.GroupMembers.Where(u => u.GroupId == id && u.UserId == currentUser && u.Status == MemberStatusFlag.Invited).ToList();
            if(invites.Any())
            {
                ViewBag.Invite = invites.First();
            }
            var isMember = db.GroupMembers.Where(u => u.GroupId == id && u.UserId == currentUser && (u.Status == MemberStatusFlag.Admin || u.Status == MemberStatusFlag.Member)).ToList();
            if (isMember.Any())
            {
                ViewBag.InGroup = true;
            }
            else
            {
                ViewBag.InGroup = false;
            }
            if(TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View(grup);
        }
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult New()
        {
            Group grup = new Group();
            return View(grup);
        }
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Edit(int id)
        {

            var grup = db.Groups.Find(id);
            return View(grup);
        }
        [HttpPut]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Edit(int id, Group editedgrup)
        {

            if (TempData.ContainsKey("message"))
                ViewBag.Message = TempData["message"];

            try
            {

                if (ModelState.IsValid)
                {
                    var grup = db.Groups.Find(id);
                    var admins = db.GroupMembers.Where(u => u.GroupId == grup.GroupId && u.Status == MemberStatusFlag.Admin).Select(u => u.UserId).ToList();
                    if (admins.Contains(User.Identity.GetUserId()) || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(grup))
                        {

                            grup.Description = editedgrup.Description;
                            grup.GroupName = editedgrup.GroupName;
                            db.SaveChanges();
                            TempData["message"] = "Grupul a fost editat!";
                            return Redirect("/Groups/Show/" + id);

                        }
                        else
                        {
                            
                            return View(editedgrup);

                        }
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti permisiunea de a edita aceast grup!";
                        return RedirectToAction("Index");
                    }


                }
                else
                {
                  
                    return View(editedgrup);
                }



            }
            catch (Exception e)
            {

            
                return View(editedgrup);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult New(Group grup)
        {


            try
            {

                if (ModelState.IsValid)
                {
                    grup.CreationTime = DateTime.Now;
                    db.Groups.Add(grup);
                    ///adaugam si o intrare in tabela GroupMember cu user-ul care a creat grupul
                    GroupMember newMember = new GroupMember();
                    newMember.GroupId = grup.GroupId;
                    newMember.UserId = User.Identity.GetUserId();
                    newMember.Status = MemberStatusFlag.Admin;
                    db.GroupMembers.Add(newMember);
                    db.SaveChanges();
                    TempData["message"] = "Grupul a fost adaugat!";
                    return RedirectToAction("Index");
                }
                else
                {
                
                    return View(grup);
                }
            }
            catch (Exception e)
            {
                
                return View(grup);
            }
        }
        [HttpDelete]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Delete(int id)
        {

            var grup = db.Groups.Find(id);
            var admins = db.GroupMembers.Where(u => u.GroupId == id && u.Status == MemberStatusFlag.Admin).Select(u => u.UserId).ToList();
            if (admins.Contains(User.Identity.GetUserId()) || User.IsInRole("Admin"))
            {
                var not = db.Notifications.Where(u => u.GroupId == id).ToList();
                foreach (var notification in not)
                {
                    db.Notifications.Remove(notification);

                }
                db.Groups.Remove(grup);
                db.SaveChanges();
                TempData["message"] = "Grupul a fost sters!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti permisiunea de a sterge acest grup!";
                return RedirectToAction("Show",new { id = id });
            }
        }
  
    }
}