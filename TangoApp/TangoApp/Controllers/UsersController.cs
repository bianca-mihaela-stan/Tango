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

            var blockedUsersFirst = db.Friendships.Where(u => u.User1Id == currentUser && u.Status == 3 && u.ActionUser == 1).Select(u => u.User2).ToList();
            var blockedUsersSecond = db.Friendships.Where(u => u.User2Id == currentUser && u.Status == 3 && u.ActionUser == 2).Select(u => u.User1).ToList();


            var allFriends = friendFirst.Union(friendSecond);
            var allBlockedUsers = blockedUsersFirst.Union(blockedUsersSecond);

            //utilizatorul curent
            var currentUserEntity = new List<ApplicationUser>();
            currentUserEntity.Add(db.Users.Find(currentUser));

            ViewBag.Friends = allFriends;
            ViewBag.BlockedUsers = allBlockedUsers;
            ViewBag.Users = users.Except(allFriends).Except(currentUserEntity).Except(allBlockedUsers).OrderByDescending(u => u.ProfileId).Take(10);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddFriend(FormCollection formData)
        {
            try
            {
                string currentUserId = User.Identity.GetUserId();
                string potentialFriendId = formData.Get("UserId");
             
                ApplicationUser sendToUser = db.Users.Find(potentialFriendId);
                if (sendToUser == null)
                {
                    TempData["message"] = "Utilizatorul nu a fost gasit!";
                    return RedirectToAction("Index","Profile");
                }
                //vrem sa verificam ca relatia intre useri nu exista deja
                var ties = db.Friendships.Where(x => (x.User1Id == potentialFriendId && x.User2Id == currentUserId) || (x.User2Id == potentialFriendId && x.User1Id == currentUserId)).ToList();
                //daca deja exista o legatura intre 2 utilizatori avem urmatoarele cazuri
                if (ties.Any())
                {

                    var alreadyExists = ties.First();
                    ///utilizatorul curent este cel care a trimis cererea de prietenie
                    if (alreadyExists.Status == 0 && alreadyExists.User1Id == currentUserId)
                    {
                        TempData["message"] = "Cererea a fost deja trimisa!";
                        return RedirectToAction("Show", "Profile", new { id = sendToUser.ProfileId });
                    }
                    //utilizatorul curent este cel care a primit deja cerere de prietenie de la sendToUser
                    if (alreadyExists.Status == 0 && alreadyExists.User1Id != currentUserId)
                    {
                        TempData["message"] = "Raspunde cererii de prietenie";
                        return Redirect("/Friendships/Index");
                    }
                    ///sendToUser este blocat
                    if (alreadyExists.Status == 3)
                    {
                        TempData["message"] = "Utilizatorul este blocat!";
                        return RedirectToAction("Show", "Profile", new { id = sendToUser.ProfileId });
                    }
                    //cineva a dat decline la cererea de prietenie
                    if (alreadyExists.Status == 2)
                    {
                        ///utilizatorul curent a dat decline la cererea de prietenie
                        if(alreadyExists.User1Id == currentUserId && alreadyExists.ActionUser == 1 || alreadyExists.User2Id == currentUserId && alreadyExists.ActionUser == 2)
                        {
                            db.Friendships.Remove(alreadyExists);
                        }
                        ///utilizatorul sendToUser a dat decline la cererea de prietenie
                        else
                        {
                            TempData["message"] = "Cererea a fost deja trimisa!";
                            return RedirectToAction("Show","Profile",new { id = sendToUser.ProfileId });
                        }
                    }
                }
                ///nu exista nicio legatura anterioara
                Friendship friendship = new Friendship();
                friendship.User1Id = currentUserId;
                friendship.User2Id = potentialFriendId;
                friendship.Status = 0;
                friendship.ActionUser = 1;
                db.Friendships.Add(friendship);
                db.SaveChanges();
                TempData["message"] = "Cererea a fost trimisa!";
                return RedirectToAction("Show", "Profile", new { id = sendToUser.ProfileId });
            }
            catch (Exception e)
            {
                TempData["message"] = "Something went wrong. Please try again later.";
                return RedirectToAction("Index","Profile");
            }
           
        }
        [HttpDelete]
        public ActionResult Unfriend(FormCollection formData)
        {


            string currentUserId = User.Identity.GetUserId();
            string formUserId = formData.Get("UserId");
            try
            {
                ApplicationUser unfriendUser = db.Users.Find(formUserId);
                if (unfriendUser == null)
                {
                    TempData["message"] = "Utilizatorul nu a fost gasit!";
                    return RedirectToAction("Index","Profile");

                }
                var friends = db.Friends.Where(u => (u.User1Id == currentUserId && u.User2Id == formUserId) || (u.User2Id == currentUserId && u.User1Id == formUserId)).ToList();
                if (!friends.Any())
                {
                    TempData["message"] = "Nu sunteti prieteni cu acest utilizator!";
                    return RedirectToAction("Show", "Profile", new { id = unfriendUser.ProfileId});
                }
                ///trebuie sa stergem si chat-ul dintre fostii prieteni
                var friend = friends.First();
                var conversationFriend = db.GroupMembers.Where(u =>  u.UserId == formUserId && u.Group.Status == GroupStatusFlag.PrivateConversation).ToList().Select(u => u.Group);
                var conversationUserCurrent = db.GroupMembers.Where(u => u.UserId == currentUserId && u.Group.Status == GroupStatusFlag.PrivateConversation).ToList().Select(u => u.Group);
                var groupconversation = conversationFriend.Intersect(conversationUserCurrent).First();
                db.Groups.Remove(groupconversation);
                db.Friends.Remove(friend);
                db.SaveChanges();
                TempData["message"] = "Nu mai sunteti prieteni!";
                return RedirectToAction("Show", "Profile", new { id = unfriendUser.ProfileId });

            }
            catch (Exception e)
            {
                TempData["message"] = "Something went wrong. Please try again later.";
                return RedirectToAction("Index", "Profile");
            }

        }
        [HttpPost]
        public ActionResult Block(FormCollection formData)
        {

            try
            {
                var userToBlockId = formData.Get("UserId");
                var currentUserId = User.Identity.GetUserId();
                var userToBlock = db.Users.Find(userToBlockId);
                if (db.Users.Find(userToBlockId) == null)
                {
                    TempData["messaege"] = "Utilizatorul nu poate fi gasit!";
                    return RedirectToAction("Index","Profile");
                }
                var friendshipList = db.Friendships.Where(u => (u.User1Id == currentUserId && u.User2Id == userToBlockId) || (u.User2Id == currentUserId && u.User1Id == userToBlockId)).ToList();
                var arefriendsList = db.Friends.Where(u => (u.User1Id == currentUserId && u.User2Id == userToBlockId) || (u.User2Id == currentUserId && u.User1Id == userToBlockId)).ToList();

                if (friendshipList.Any())
                {
                    var friendship = friendshipList.First();
                    if (friendship.Status == 3)
                    {
                        TempData["message"] = "Utilizatorul este deja blocat!";
                        return RedirectToAction("Show", "Profile", new { id = userToBlock.ProfileId});
                    }
                    friendship.Status = 3;
                    if (currentUserId == friendship.User1Id)
                        friendship.ActionUser = 1;
                    else
                        friendship.ActionUser = 2;
                }
                else
                {

                    Friendship blocked = new Friendship();
                    blocked.User1Id = currentUserId;
                    blocked.User2Id = userToBlockId;
                    blocked.Status = 3;
                    blocked.ActionUser = 1;
                    db.Friendships.Add(blocked);

                    if (arefriendsList.Any())
                    {

                        var arefriends = arefriendsList.First();
                        db.Friends.Remove(arefriends);
                    }

                }
                db.SaveChanges();
                TempData["message"] = "Utilizatorul a fost blocat!";
                return RedirectToAction("Show", "Profile", new { id = userToBlock.ProfileId });
            }
            catch (Exception e)
            {
                TempData["message"] = "Am intampinat o eroare" + e;
                return RedirectToAction("Index","Profile");
            }


        }
        [HttpDelete]
        public ActionResult Unblock(FormCollection formData)
        {

            try
            {

                var userToUnblockId = formData.Get("UserId");
                var userToUnblock = db.Users.Find(userToUnblockId);
                if (db.Users.Find(userToUnblockId) == null)
                {
                    TempData["message"] = "Utilizatorul nu a putut fi gasit!";
                    return RedirectToAction("Index","Profile");
                }
                var currentUserId = User.Identity.GetUserId();
                var friendshipList = db.Friendships.Where(u => (u.User1Id == currentUserId && u.User2Id == userToUnblockId) || (u.User2Id == currentUserId && u.User1Id == userToUnblockId)).ToList();
                if (!friendshipList.Any() || (friendshipList.Any() && friendshipList.First().Status != 3))
                {
                    TempData["message"] = "Utilizatorul nu este blocat!";
                    return RedirectToAction("Show", "Profile", new { id = userToUnblock.ProfileId });
                }
                var friendship = friendshipList.First();
                db.Friendships.Remove(friendship);
                db.SaveChanges();
                TempData["message"] = "Utilizatorul a fost deblocat!";
                return RedirectToAction("Show", "Profile", new { id = userToUnblock.ProfileId });

            }
            catch (Exception e)
            {

                TempData["message"] = "Am intampinat o eroare" + e;
                return RedirectToAction("Index","Profile");

            }


        }
    }
}