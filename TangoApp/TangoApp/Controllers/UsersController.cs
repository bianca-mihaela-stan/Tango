using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class UsersController : Controller
    {
        // GET: Users
        public ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            var users = db.Users;
            ViewBag.Users = users;
            if(TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddFriend(FormCollection formData)
        {

            string currentUserId = User.Identity.GetUserId();
            string potentialFriendId = formData.Get("UserId");
            ApplicationUser sendToUser = db.Users.Find(potentialFriendId);
            if(sendToUser == null)
            {
                TempData["message"] = "Utilizatorul nu a fost gasit!";
                return RedirectToAction("Index");
            }
            //vrem sa verificam ca relatia intre useri nu exista deja
            var ties = db.Friendships.Where(x => (x.User1Id == potentialFriendId && x.User2Id == currentUserId) || (x.User2Id == potentialFriendId && x.User1Id == currentUserId));
            /*if(!ties.Any())
            {
                TempData["message"] = "Cererea a fost deja trimisa!";
                return RedirectToAction("Index");
            }*/
            try
            {
                Friendship friendship = new Friendship();
                friendship.User1Id = currentUserId;
                friendship.User2Id = potentialFriendId;
                friendship.Status = 0;
                friendship.ActionUser = 1;
                db.Friendships.Add(friendship);
                db.SaveChanges();
                TempData["message"] = "Cererea a fost trimisa!";
            }
            catch(Exception e)
            {
                TempData["message"] = "Something went wrong. Please try again later." + e;
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}