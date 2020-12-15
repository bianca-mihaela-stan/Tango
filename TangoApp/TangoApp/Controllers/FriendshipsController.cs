using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class FriendshipsController : Controller
    {
        // GET: Friendships
        public ApplicationDbContext db = new ApplicationDbContext();
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Index()
        {
            var currentUser = User.Identity.GetUserId();
            var friendrequests = from req in db.Friendships
                                 where req.User2Id == currentUser && req.Status == 0
                                 select req;
            ViewBag.Friendrequests = friendrequests.ToList();
            if(TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }
        public ActionResult Accept(int id)
        {

            ///primim id-ul friendship-ului
            ///stergem intrarea din Friendships si adaugam o noua intrare corespunzatoare
            ///in Friends
            try
            {
                var friendship = db.Friendships.Find(id);
                db.Friendships.Remove(friendship);
                Friend newfriend = new Friend();
                newfriend.User1Id = friendship.User1Id;
                newfriend.User2Id = friendship.User2Id;
                newfriend.StartDate = DateTime.Now;
                db.Friends.Add(newfriend);
                db.SaveChanges();
                var user = db.Users.Find(newfriend.User1Id);
                TempData["message"] = "Acum esti prieten cu " + user.Email;
                return RedirectToAction("Index");
            }
            catch(Exception e)
            {
                TempData["message"] = "Am intampinat o eroare";
                return RedirectToAction("Index");
            }
        }
        [HttpPut]
        public ActionResult Decline(int id)
        {

            try
            {
                var friendship = db.Friendships.Find(id);
                friendship.Status = 2;
                friendship.ActionUser = 2;
                db.SaveChanges();
                TempData["message"] = "Cererea a fost respinsa";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                TempData["message"] = "Am intampinat o eroare";
                return RedirectToAction("Index");
            }


        }

    }
}