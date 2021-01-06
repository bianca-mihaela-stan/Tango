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
            var usersInGroup = db.GroupMembers.Include("User").Where(u => u.GroupId == id && u.Status != MemberStatusFlag.Pending).ToList().Select(u => u.User);
            var usersPending = db.GroupMembers.Include("User").Where(u => u.GroupId == id && u.Status == MemberStatusFlag.Pending).ToList().Select(u => u.User);

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
        //de terminat!
        /*public ActionResult ShowJoinRequests(int id)
        {
            var requests = db.Notifications.Where(u => u.GroupId == id && u.)
            return View();
        }*/
        [Authorize(Roles = "Admin,User,Editor")]
        [HttpPost]
        public ActionResult JoinRequest(int id)
        {
            var grup = db.Groups.Find(id);
            var currentUserId = User.Identity.GetUserId();
            if (grup == null)
            {
                TempData["message"] = "Grupul nu exista!";
                return RedirectToAction("Index", "Groups");
            }
            //vrem sa verificam daca userul se afla deja in grup sau daca a trimis deja cerere
            var isInGroup = db.GroupMembers.Where(u => u.GroupId == grup.GroupId && u.Status != MemberStatusFlag.Pending && u.UserId == currentUserId).ToList();
            if(isInGroup.Any())
            {
                TempData["message"] = "Faci deja parte din acest grup!";
                return RedirectToAction("Index", "Groups");
            }
            
            var inPending = db.GroupMembers.Where(u => u.GroupId == grup.GroupId && u.Status == MemberStatusFlag.Pending && u.UserId == currentUserId).ToList();
            if(inPending.Any())
            {
                TempData["message"] = "Deja ai trimis cerere de inscriere!";
                return RedirectToAction("Index", "Groups");
            }
            try
            {
                var joinrequest = new GroupMember();
                joinrequest.GroupId = grup.GroupId;
                joinrequest.UserId = currentUserId;
                joinrequest.Status = MemberStatusFlag.Pending;
                db.GroupMembers.Add(joinrequest);
                db.SaveChanges();

                ///trebuie sa trimitem o notificare tuturor adminilor grupului
                var adminsIds = db.GroupMembers.Where(u => u.GroupId == grup.GroupId && u.Status == MemberStatusFlag.Admin).ToList().Select(u => u.UserId);
                foreach(var adminId in adminsIds)
                {
                    Notification notification = new Notification
                    {
                        Time = DateTime.Now,
                        Seen = false,
                        UserReceiveId = adminId,
                        UserSendId = currentUserId,
                        Type = NotificationFlag.JoinRequest,
                        GroupId = grup.GroupId
                    };
                    db.Notifications.Add(notification);

                }
                db.SaveChanges();

                TempData["message"] = "Cererea a fost trimisa!";
                return RedirectToAction("Index","Groups");
            }
            catch(Exception e)
            {
                TempData["message"] = "Something bad happened.";
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
        //de terminat!
        /*[HttpPost]
        public ActionResult AcceptRequestFromUser(int id)
        {
            ///primesti o intrare din GroupMembers
            var request = db.GroupMembers.Find(id);
            var admins = db.GroupMembers.Where(u => u.GroupId == request.GroupId && u.Status == MemberStatusFlag.Admin).ToList().Select(u => u.UserId);
            var currentUserId = User.Identity.GetUserId();

            if(admins.Contains(currentUserId))
            {
                request.Status = MemberStatusFlag.Member;

                ///trebuie sa mergem si sa schimbam tipul de notificare - acum userul este un nou membru
                ///al  grupului
                return RedirectToAction("JoinRequests");
            }
        }
        */

    }
}