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