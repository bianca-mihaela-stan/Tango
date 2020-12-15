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
            return View();
        }
        public ActionResult Accept(int id)
        {

            ///primim id-ul friendship-ului
            //var friendship = db.Friendships.Find(id);
            //db.Friendships.Remove(friendship);
           // Friend newfriend = new Friend();
            //newfriend.User1Id = friendship.User1Id;
            //newfriend.User2Id = friendship.User2Id;
            //newfriend.StartDate = DateTime.Now;
            //db.Friends.Add(newfriend);

            
            return View();
        }

    }
}