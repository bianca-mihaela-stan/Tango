using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class GroupMembersController : Controller
    {
        // GET: GroupMembers
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(int id)
        {
            var usersInGroup = db.GroupMembers.Include("User").Where(u => u.GroupId == id && u.Status != MemberStatusFlag.Pending).ToList();
            var usersPending = db.GroupMembers.Include("User").Where(u => u.GroupId == id && u.Status == MemberStatusFlag.Pending).ToList();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            ViewBag.afisareButoane = false;
            var admins = db.GroupMembers.Where(u => u.GroupId == id && u.Status == MemberStatusFlag.Admin).Select(u => u.UserId).ToList();
            if (User.IsInRole("Admin") || admins.Contains(User.Identity.GetUserId()))
            {
                ViewBag.afisareButoane = true;
            }
            ViewBag.esteAdmin = User.IsInRole("Admin");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
            ViewBag.UsersInGroup = usersInGroup;
            ViewBag.UsersPending = usersPending;
            return View();
        }
        [Authorize(Roles = "Admin,User,Editor")]
        [HttpPost]
        public ActionResult JoinRequest(int id)
        {
            var grup = db.Groups.Find(id);
            if (grup == null)
            {
                Session["message"] = "Grupul nu exista!";
                return RedirectToAction("Index", "Groups");
            }
            try
            {
                var joinrequest = new GroupMember();
                joinrequest.GroupId = grup.GroupId;
                joinrequest.UserId = User.Identity.GetUserId();
                joinrequest.Status = MemberStatusFlag.Pending;
                db.GroupMembers.Add(joinrequest);
                db.SaveChanges();
                Session["message"] = "Cererea a fost trimisa!";
                return RedirectToAction("Index","Groups");
            }
            catch(Exception e)
            {
                Session["message"] = "Something bad happened.";
                return RedirectToAction("Index", "Groups");
            }
        }
        [HttpPost]
        public ActionResult AskToJoin(string UserId, int GrupId)
        {

            var grup = db.Groups.Find(GrupId);
            if (grup == null)
            {
                Session["message"] = "Grupul nu exista!";
                return RedirectToAction("Index", "Groups");
            }
            try
            {
                var joinrequest = new GroupMember();
                joinrequest.GroupId = GrupId;
                joinrequest.UserId = UserId;
                joinrequest.Status = MemberStatusFlag.Pending;
                db.GroupMembers.Add(joinrequest);
                db.SaveChanges();
                Session["message"] = "Cererea a fost trimisa!";
                return RedirectToAction("Index", "Groups");
            }
            catch (Exception e)
            {
                Session["message"] = "Something bad happened.";
                return RedirectToAction("Index", "Groups");
            }

        }

    }
}