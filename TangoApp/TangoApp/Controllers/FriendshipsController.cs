using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    [Authorize(Roles ="User,Editor,Admin")]
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
                if(friendship == null)
                {
                    TempData["message"] = "Nu aveti nicio cerere de prietenie de la acest user!";
                    return RedirectToAction("Index");
                }
                
                Friend newfriend = new Friend();
                newfriend.User1Id = friendship.User1Id;
                newfriend.User2Id = friendship.User2Id;
                newfriend.StartDate = DateTime.Now;
                db.Friends.Add(newfriend);


                db.Friendships.Remove(friendship);
                var foundfriends = db.Friends.Find(newfriend.FriendId);
                if (foundfriends == null)
                {
                    TempData["message"] = "Relatia nu a putut fi gasita!";
                    return RedirectToAction("Index");
                }
                var user = db.Users.Find(newfriend.User1Id);
                if(user == null)
                {
                    TempData["message"] = "Userul nu a putut fi gasit!";
                    return RedirectToAction("Index");
                }
                db.SaveChanges();
                ///cream un grup de tipul conversatie privata
                Group privateconv = new Group
                {
                    Status = GroupStatusFlag.PrivateConversation,
                    CreationTime = DateTime.Now,
                    GroupName = "Private"
                };
                db.Groups.Add(privateconv);
                GroupMember privateConvUsersFirst = new GroupMember();
                privateConvUsersFirst.Status = MemberStatusFlag.Member;
                privateConvUsersFirst.GroupId = privateconv.GroupId;
                privateConvUsersFirst.UserId = newfriend.User1Id;

                GroupMember privateConvUsersSecond = new GroupMember();
                privateConvUsersSecond.Status = MemberStatusFlag.Member;
                privateConvUsersSecond.GroupId = privateconv.GroupId;
                privateConvUsersSecond.UserId = newfriend.User2Id;

               
                db.GroupMembers.Add(privateConvUsersFirst);
                db.GroupMembers.Add(privateConvUsersSecond);
                db.SaveChanges();
                TempData["message"] = "Acum esti prieten cu " + user.Email;
                return RedirectToAction("Index");
            }
            catch(Exception e)
            {
                TempData["message"] = "Am intampinat o eroare" + e;
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