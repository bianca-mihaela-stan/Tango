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
            var users = db.Users.ToArray();
            var currentUser = User.Identity.GetUserId();
            var friendFirst = db.Friends.Include("Users").Where(u => u.User1Id == currentUser).Select(u => u.User2).ToList();
            var friendSecond = db.Friends.Include("Users").Where(u => u.User2Id == currentUser).Select(u => u.User1).ToList();

            ///var blockedUsers = db.Friendships.Where(u => u.User1Id == currentUser && u.Status == 3 && u.ActionUser == 1).Select(u => u.User2).ToList();

            var allFriends = friendFirst.Union(friendSecond);
            var currentUserEntity = new List<ApplicationUser>();
            currentUserEntity.Add(db.Users.Find(currentUser));

            ViewBag.Friends = allFriends;
            ViewBag.Users = users.Except(allFriends).Except(currentUserEntity);
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
            var ties = db.Friendships.Where(x => (x.User1Id == potentialFriendId && x.User2Id == currentUserId) || (x.User2Id == potentialFriendId && x.User1Id == currentUserId)).ToList();
            if(ties.Any())
            {
                TempData["message"] = "Cererea a fost deja trimisa!";
                return RedirectToAction("Index");
            }
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
        [HttpPost]
        public ActionResult Block(FormCollection formData)
        {

            try
            {
                var userToBlockId = formData.Get("UserId");
                var currentUserId = User.Identity.GetUserId();
                var friendshipList = db.Friendships.Where(u => (u.User1Id == currentUserId && u.User2Id == userToBlockId) || (u.User2Id == currentUserId && u.User1Id == userToBlockId)).ToList();
                var arefriendsList = db.Friends.Where(u => (u.User1Id == currentUserId && u.User2Id == userToBlockId) || (u.User2Id == currentUserId && u.User1Id == userToBlockId)).ToList();
                if (friendshipList.Any())
                {
                    var friendship = friendshipList.First();
                    if (friendship.Status == 3)
                    {
                        TempData["message"] = "Utilizatorul este deja blocat!";
                        return RedirectToAction("Index");
                    }
                    friendship.Status = 3;
                    if (currentUserId == friendship.User1Id)
                        friendship.ActionUser = 1;
                    else
                        friendship.ActionUser = 2;
                }
                if (arefriendsList.Any())
                {
                    var arefriends = arefriendsList.First();
                    Friendship blocked = new Friendship();
                    blocked.User1Id = currentUserId;
                    blocked.User2Id = userToBlockId;
                    blocked.Status = 3;
                    blocked.ActionUser = 1;
                    db.Friends.Remove(arefriends);
                    db.SaveChanges();
                    db.Friendships.Add(blocked);
                    db.SaveChanges();
                }
                
                TempData["message"] = "Utilizatorul a fost blocat!";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                TempData["message"] = "Am intampinat o eroare" + e;
                return RedirectToAction("Index");
            }


        }
    }
}