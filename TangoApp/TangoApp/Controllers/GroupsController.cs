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

        [Authorize(Roles = "User,Editor,Admin")]
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


        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult ShowPrivateConv(int id)
        {
            var currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.Find(currentUserId);
            var friendId = db.GroupMembers.Find(id).UserId;
            var Group= db.GroupMembers.Find(id).Group;
            var friend = db.Users.Find(friendId);
            if(friend == null)
            {
                TempData["message"] = "Utilizatorul nu exista!";
                return RedirectToAction("Index", "Users");
            }
            if(Group == null)
            {
                TempData["message"] = "Nu esti prieten cu acest utilizator";
                return RedirectToAction("Show", "Profile", new {id = friendId });
            }
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            ViewBag.utilizatorCurent = User.Identity.GetUserId();
            ViewBag.UserFriend = friend;
            ViewBag.esteAdmin = User.IsInRole("Admin");
            return View(Group);

        }
       
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Show(int id)
        {
            var grup = db.Groups.Find(id);
            var currentUser = User.Identity.GetUserId();
            var invites = db.GroupMembers.Where(u => u.GroupId == id && u.UserId == currentUser && u.Status == MemberStatusFlag.Invited).ToList();
            if (invites.Select(u => u.UserId).Contains(currentUser))
            {
                ViewBag.Invitation = db.GroupMembers.First(a => a.GroupId == id && a.UserId == currentUser);
            }
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
            ViewBag.esteAdmin = User.IsInRole("Admin");
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
            var admins = db.GroupMembers.Where(u => u.GroupId == grup.GroupId && u.Status == MemberStatusFlag.Admin).Select(u => u.UserId).ToList();
            if (admins.Contains(User.Identity.GetUserId()) || User.IsInRole("Admin"))
            {
                return View(grup);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui grup in care nu sunteti admin!";
                return RedirectToAction("Index");
            }
           
        }
        [HttpPut]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Edit(int id, Group editedgrup)
        {

            

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

                TempData["message"] = "Something went terribly wrong!";
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
                    grup.Status = GroupStatusFlag.MessageGroup;
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
                TempData["message"] = "Something went terribly wrong!";
                return View(grup);
            }
        }
        [HttpDelete]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Delete(int id)
        {
            try
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
                    return RedirectToAction("Show", new { id = id });
                }
            }
            catch(Exception e)
            {
                TempData["message"] = "Something went terribly wrong!";
                return RedirectToAction("Show", new { id = id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult NewMessage(int GroupId, Message mess)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    Console.Write("option 1");
                    mess.Date = DateTime.Now;
                    mess.UserId = User.Identity.GetUserId();
                    mess.GroupId = GroupId;
                    db.Messages.Add(mess);
                    db.SaveChanges();
                    TempData["message"] = "Mesajul a fost adaugat!";
                    var currentUser = User.Identity.GetUserId();
                    var friendId = db.GroupMembers.First(a => a.GroupId == mess.GroupId && a.UserId != mess.UserId).UserId;
                    var conversationId = db.GroupMembers.First(a => a.GroupId == GroupId && a.UserId == currentUser).GroupMemberId;
                    Console.WriteLine(conversationId);
                    if(db.Groups.Find(GroupId).Status==GroupStatusFlag.PrivateConversation)
                        return RedirectToAction("ShowPrivateConv", "Groups", new {id= conversationId });
                    else
                        return RedirectToAction("Show", "Groups", new { id = GroupId });
                    //return View("Home", "Index");
                }
                else
                {
                    Console.Write("option 2");
                    Group grup = db.Groups.Find(mess.GroupId);
                    ViewBag.Message = mess; var friendId = db.GroupMembers.First(a => a.GroupId == mess.GroupId && a.UserId != mess.UserId).UserId;
                    var conversationId = db.GroupMembers.First(a => a.GroupId == GroupId && a.UserId == User.Identity.GetUserId());
                    if (db.Groups.Find(GroupId).Status == GroupStatusFlag.PrivateConversation)
                        return RedirectToAction("ShowPrivateConv", "Groups", new { id = conversationId });
                    else
                        return RedirectToAction("Show", "Groups", new { id = GroupId });
                    //return View("Home", "Index");
                }

            }
            catch (Exception e)
            {
                Group grup = db.Groups.Find(mess.GroupId);
                return View("Show", grup);
            }


        }


    }
}
