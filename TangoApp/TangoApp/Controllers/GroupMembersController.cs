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
    public class GroupMembersController : Controller
    {
        // GET: GroupMembers
        private ApplicationDbContext db = new ApplicationDbContext();
        //un utilizator vrea sa vada membrii grupului

        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Show(int id)
        {
            var usersInGroup = db.GroupMembers.Include("User").Where(u => u.GroupId == id &&  u.Status == MemberStatusFlag.Member).ToList();
            var usersPending = db.GroupMembers.Include("User").Where(u => u.GroupId == id && u.Status == MemberStatusFlag.Pending).ToList();
            var usersInvited = db.GroupMembers.Include("User").Where(u => u.GroupId == id && u.Status == MemberStatusFlag.Invited).ToList();
           
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            ViewBag.afisare= false;
            var admins = db.GroupMembers.Where(u => u.GroupId == id && u.Status == MemberStatusFlag.Admin).ToList();
            if (User.IsInRole("Admin") || admins.Select(u => u.UserId).Contains(User.Identity.GetUserId()))
            {
                ViewBag.afisare = true;
            }
            ViewBag.esteAdmin = User.IsInRole("Admin");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
            ViewBag.UsersInGroup = usersInGroup;
            ViewBag.UsersPending = usersPending;
            ViewBag.UsersInvited = usersInvited;
            ViewBag.Admins = admins;
            ViewBag.GroupMembersId = id;
            return View();
        }
        // un utilizator trimite cerere de inscriere
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
            var isInGroup = db.GroupMembers.Where(u => u.GroupId == grup.GroupId && (u.Status == MemberStatusFlag.Member || u.Status == MemberStatusFlag.Admin) &&  u.UserId == currentUserId).ToList();
            if(isInGroup.Any())
            {
                TempData["message"] = "Faci deja parte din acest grup!";
                return RedirectToAction("Index", "Groups");
            }
            
            var inPending = db.GroupMembers.Where(u => u.GroupId == grup.GroupId && (u.Status == MemberStatusFlag.Pending || u.Status == MemberStatusFlag.Invited) && u.UserId == currentUserId).ToList();
            if(inPending.Any())
            {
                var status = inPending.First().Status;
                if (status == MemberStatusFlag.Pending)
                    TempData["message"] = "Deja ai trimis cerere de inscriere!";
                else
                    TempData["message"] = "Ai fost invitat in grup deja!";
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
        ///un membru vrea sa invite pe cineva in grup
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Invite(string UserId, int GrupId)
        {

            var grup = db.Groups.Find(GrupId);
            if (grup == null)
            {
                TempData["message"] = "Grupul nu exista!";
                return RedirectToAction("Index", "Groups");
            }
            var currentUserId = User.Identity.GetUserId();
            var members = db.GroupMembers.Where(u => u.GroupId == GrupId &&( u.Status == MemberStatusFlag.Admin || u.Status == MemberStatusFlag.Member)).ToList().Select(u => u.UserId);
            var pendingorinvited = db.GroupMembers.Where(u => u.GroupId == GrupId && !(u.Status == MemberStatusFlag.Admin || u.Status == MemberStatusFlag.Member)).ToList().Select(u => u.UserId);
            if (!members.Contains(currentUserId) && !User.IsInRole("Admin"))
            {
                TempData["message"] = "Nu ai dreptul de a invita pe cineva!";
                return RedirectToAction("Search", "GroupMembers", new { id = GrupId });
            }
            if(members.Contains(UserId))
            {
                TempData["message"] = "Utilizatorul face deja parte din grup!";
                return RedirectToAction("Search", "GroupMembers", new { id = GrupId });
            }
            if(pendingorinvited.Contains(UserId))
            {
                TempData["message"] = "Utilizatorul a fost deja invitat sau a trimis deja cerere!";
                return RedirectToAction("Search", "GroupMembers", new { id = GrupId });
            }
        
            try
            {
                var joinrequest = new GroupMember();
                joinrequest.GroupId = GrupId;
                joinrequest.UserId = UserId;
                joinrequest.Status = MemberStatusFlag.Invited;
                db.GroupMembers.Add(joinrequest);
                Notification notification = new Notification();
                notification.GroupId = GrupId;
                notification.UserReceiveId = UserId;
                notification.UserSendId = currentUserId;
                notification.Type = NotificationFlag.AskToJoinRequest;
                notification.Time = DateTime.Now;
                notification.Seen = false;
                db.Notifications.Add(notification);
                db.SaveChanges();
                TempData["message"] = "Invitatia a fost trimisa!";
                return RedirectToAction("Search", "GroupMembers", new { id = GrupId });
            }
            catch (Exception e)
            {
                TempData["message"] = "Something bad happened.";
                return RedirectToAction("Index", "Groups");
            }

        }
        
        // un admin vrea sa accepte cuiva cererea de inscriere
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult AcceptRequest(int id)
        {
            ///primesti o intrare din GroupMembers
            var request = db.GroupMembers.Find(id);
            if(request == null)
            {
                TempData["message"] = "Cererea nu exista!";
                return RedirectToAction("Index", "Groups");
            }
            var admins = db.GroupMembers.Where(u => u.GroupId == request.GroupId && u.Status == MemberStatusFlag.Admin).ToList().Select(u => u.UserId);
            var currentUserId = User.Identity.GetUserId();

            if(admins.Contains(currentUserId) || User.IsInRole("Admin"))
            {
                request.Status = MemberStatusFlag.Member;
                var notifications = db.Notifications.Where(u => u.GroupId == request.GroupId && u.UserSendId == request.UserId).ToList();
                foreach(var not in notifications)
                {
                    not.Type = NotificationFlag.NewMember;
                    not.Time = DateTime.Now;
                    not.Seen = false;
                }
                //trebuie sa ii trimitem notificare si membrului care a fost acceptat in grup
                Notification notification = new Notification();
                notification.GroupId = request.GroupId;
                notification.UserReceiveId = request.UserId;
                notification.UserSendId = currentUserId;
                notification.Type = NotificationFlag.AddedToGroup;
                notification.Time = DateTime.Now;
                notification.Seen = false;
                db.Notifications.Add(notification);
                db.SaveChanges();
                TempData["message"] = "Utilizatorul a fost adaugat in grup.";
                return RedirectToAction("Show","GroupMembers", new { id = request.GroupId });
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul de a include un membru in grup.";
                return RedirectToAction("Show", "GroupMembers", new { id = request.GroupId });
            }
        }
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult AcceptInvitation(int id)
        {

            ///primesti o intrare din GroupMembers
            var request = db.GroupMembers.Find(id);
            if (request == null)
            {
                TempData["message"] = "Cererea nu exista!";
                return RedirectToAction("Index", "Groups");
            }
            var currentUser = User.Identity.GetUserId();
            if(currentUser == request.UserId)
            {
                request.Status = MemberStatusFlag.Member;
                var notifications = db.Notifications.Where(u => u.GroupId == request.GroupId && u.UserReceiveId== request.UserId).ToList();
                var not = notifications.First();
                db.Notifications.Remove(not);
              

                //adminii o sa primeasca notificare ca ai intrat in grup
                var adminsIds = db.GroupMembers.Where(u => u.GroupId == request.GroupId && u.Status == MemberStatusFlag.Admin).ToList().Select(u => u.UserId);
                foreach (var adminId in adminsIds)
                {
                    Notification notification = new Notification
                    {
                        Time = DateTime.Now,
                        Seen = false,
                        UserReceiveId = adminId,
                        UserSendId = currentUser,
                        Type = NotificationFlag.NewMember,
                        GroupId = request.GroupId
                    };
                    db.Notifications.Add(notification);

                }
                db.SaveChanges();
                TempData["message"] = "Ai intrat in grup!";
                return RedirectToAction("Show", "Groups", new { id = request.GroupId });
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul de a accepta invitatia";
                return RedirectToAction("Show", "Groups", new { id = request.GroupId });
            }
        }
        [HttpDelete]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult DeclineRequest(int id)
        {

            ///primesti o intrare din GroupMembers
            var request = db.GroupMembers.Find(id);
            if (request == null)
            {
                TempData["message"] = "Cererea nu exista!";
                return RedirectToAction("Show", "GroupMembers", new { id = request.GroupId });
            }
            var admins = db.GroupMembers.Where(u => u.GroupId == request.GroupId && u.Status == MemberStatusFlag.Admin).ToList().Select(u => u.UserId);
            var currentUserId = User.Identity.GetUserId();

            if (admins.Contains(currentUserId) || User.IsInRole("Admin"))
            {


                db.GroupMembers.Remove(request);
                var notifications = db.Notifications.Where(u => u.GroupId == request.GroupId && u.UserSendId == request.UserId).ToList();
                foreach (var not in notifications)
                {
                    db.Notifications.Remove(not);
                }
                db.SaveChanges();
                TempData["message"] = "Cererea a fost stearsa!";
                return RedirectToAction("Show", "GroupMembers", new { id = request.GroupId });
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul de a sterge o cerere de inscriere.";
                return RedirectToAction("Show", "GroupMembers", new { id = request.GroupId });
            }



        }
        [HttpDelete]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult DeclineInvitation(int id)
        {
            ///primesti o intrare din GroupMembers
            var request = db.GroupMembers.Find(id);
            if (request == null)
            {
                TempData["message"] = "Invitatia nu exista!";
                return RedirectToAction("Index", "Groups");
            }
            var currentUser = User.Identity.GetUserId();
            if (currentUser == request.UserId)
            {
                db.GroupMembers.Remove(request);
                db.SaveChanges();
                TempData["message"] = "Ai sters invitatia!";
                return RedirectToAction("Show", "Groups", new { id = request.GroupId });
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul de a sterge invitatia";
                return RedirectToAction("Show", "Groups", new { id = request.GroupId });
            }

        }
        [HttpDelete]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult LeaveGroup(int id)
        {
            var currentUserId = User.Identity.GetUserId();
            var inGroup = db.GroupMembers.Where(u => u.GroupId == id && u.UserId == currentUserId).ToList();
            if(!inGroup.Any())
            {
                TempData["message"] = "Nu faceti parte din acest grup!";
                return RedirectToAction("Show", "Groups", new { id = id});
            }
            var relation = inGroup.First();
            //avand in vedere ca nu poti sa ajungi in remove member cu propriul tau id,
            //cand ajungi sa fii singur in grup, poti sa il stergi sau poti sa il parasesti
            //daca il parasesti grupul trebuie sters
            db.GroupMembers.Remove(relation);
            db.SaveChanges();
            var countMembers = db.GroupMembers.Where(u => u.GroupId == id).ToList();
            var countAdmins = db.GroupMembers.Where(u => u.GroupId == id && u.Status == MemberStatusFlag.Admin).ToList();
            if (!countMembers.Any())
            {
                var grup = db.Groups.Find(id);
                var not = db.Notifications.Where(u => u.GroupId == id).ToList();
                foreach(var notification in not)
                {
                    db.Notifications.Remove(notification);

                }
                db.Groups.Remove(grup);
                db.SaveChanges();
            }
            else
            {
                if(!countAdmins.Any())
                {
                    //o sa il facem admin pe primul membru ramas care a ramas in grup
                    try
                    {
                        var adminToBe = countMembers.OrderBy(u => u.JoinDate).First();
                        adminToBe.Status = MemberStatusFlag.Admin;

                        Notification notification = new Notification();
                        notification.GroupId = id;
                        notification.UserSendId = currentUserId;
                        notification.UserReceiveId = adminToBe.UserId;
                        notification.Type = NotificationFlag.MadeAdmin;
                        notification.Time = DateTime.Now;
                        notification.Seen = false;
                        db.Notifications.Add(notification);
                        db.SaveChanges();
                    }
                    catch(Exception e)
                    {
                        TempData["message"] = "A intervenit o eroare!";
                        return RedirectToAction("Show", "GroupMembers", new { id = id });
                    }

                }
            }
            TempData["message"] = "Ati parasit grupul!";
            return RedirectToAction("Index", "Groups");
        }
        [HttpDelete]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult RemoveMember(int id)
        {

            ///primesti o intrare din GroupMembers
            var request = db.GroupMembers.Find(id);
            if (request == null || (request.Status != MemberStatusFlag.Admin && request.Status != MemberStatusFlag.Member))
            {
                TempData["message"] = "Membrul nu face parte din grup!";
                return RedirectToAction("Show", "GroupMembers", new { id = request.GroupId });
            }
          
            var admins = db.GroupMembers.Where(u => u.GroupId == request.GroupId && u.Status == MemberStatusFlag.Admin).ToList().Select(u => u.UserId);
            var currentUserId = User.Identity.GetUserId();

            if (admins.Contains(currentUserId) || User.IsInRole("Admin"))
            {

               
                var notifications = db.Notifications.Where(u => u.GroupId == request.GroupId && u.UserSendId == request.UserId).ToList();
                foreach (var not in notifications)
                {
                    db.Notifications.Remove(not);
                }
                Notification notification = new Notification();
                notification.GroupId = request.GroupId;
                notification.UserReceiveId = request.UserId;
                notification.UserSendId = currentUserId;
                notification.Type = NotificationFlag.RemovedFromGroup;
                notification.Time = DateTime.Now;
                notification.Seen = false;
                db.Notifications.Add(notification);
                db.GroupMembers.Remove(request);
                db.SaveChanges();
                TempData["message"] = "Membrul a fost scos!";
                return RedirectToAction("Show", "GroupMembers", new { id = request.GroupId });
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul de a scoate un membru";
                return RedirectToAction("Show", "GroupMembers", new { id = request.GroupId });
            }

        }
        [HttpPut]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult MakeAdmin(int membergroupid)
        {
            var relation = db.GroupMembers.Find(membergroupid);
            if (relation == null)
            {
                TempData["messsage"] = "A aparut o eroare.";
                return RedirectToAction("Index", "Groups");
            }
            var id = relation.GroupId;

            try
            {

                
                var currentUser = User.Identity.GetUserId();
                //trebuie verificat ca utilizatorul curent sa fie admin in grupul respectiv
                var isAdmin = db.GroupMembers.Where(u => u.GroupId == id && u.UserId == currentUser && u.Status == MemberStatusFlag.Admin).ToList();
                if (!isAdmin.Any() && !User.IsInRole("Admin"))
                {
                    TempData["messsage"] = "Nu aveti dreptul de a face admini!";
                    return RedirectToAction("Show", "GroupMembers", new { id = id });

                }
                if (relation.Status == MemberStatusFlag.Pending || relation.Status == MemberStatusFlag.Invited)
                {
                    TempData["messsage"] = "Utilizatorul nu face parte din grup";
                    return RedirectToAction("Show", "GroupMembers", new { id = id });
                }
                if (relation.Status == MemberStatusFlag.Admin)
                {

                    TempData["messsage"] = "Utilizatorul e deja admin!";
                    return RedirectToAction("Show", "GroupMembers", new { id = id });
                }
                relation.Status = MemberStatusFlag.Admin;
                Notification notification = new Notification();
                notification.GroupId = id;
                notification.UserReceiveId = relation.UserId;
                notification.UserSendId = currentUser;
                notification.Type = NotificationFlag.MadeAdmin;
                notification.Time = DateTime.Now;
                notification.Seen = false;
                db.Notifications.Add(notification);
                db.SaveChanges();
                TempData["message"] = "Utilizatorul a fost facut admin!";
                return RedirectToAction("Show", "GroupMembers", new { id = id });

            }
            catch(Exception e)
            {

                TempData["message"] = "A intervenit o eroare!";
                return RedirectToAction("Show", "GroupMembers", new { id = id });
            }

        }
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Search(int id)
        {
            //nu vreau sa adaug useri care sunt deja in grup
            List<string> useri_in_grup =(from x in db.GroupMembers
                                        where x.GroupId == id
                                        select x.UserId).ToList();
            //profilurile userilor care nu sunt deja in grup
            List<Profile> profiles = db.Profiles.Include("User").Include("Country").Include("City").Where(a => !useri_in_grup.Contains(a.UserId)).ToList();
            var search = "";
            var number_of_profiles_perpage = 10;
            if (Request.Params.Get("search") != null)
            {
                //trim whitespace from search string
                search = Request.Params.Get("search").Trim();
                //search in posts (content, name of the creator)
                List<int> postIds = db.Posts.Where(
                    at => at.Text.Contains(search)
                    || at.User.UserName.Contains(search)
                    ).Select(a => a.PostId).ToList();

                profiles = db.Profiles.Where(
                    at => at.User.UserName.Contains(search)).OrderBy(a => a.User.UserName).ToList();



            }

            var totalItems = profiles.Count();
            var currentPageProfiles = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPageProfiles.Equals(0))
            {
                offset = (currentPageProfiles - 1) * number_of_profiles_perpage;
            }
            var paginatedProfiles = profiles.OrderBy(a => a.User.UserName).Skip(offset).Take(number_of_profiles_perpage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)number_of_profiles_perpage);
            ViewBag.Profiles = paginatedProfiles;
            ViewBag.SearchString = search;
            ViewBag.GroupMembersId = id;

            return View();
        }
        /*[NonAction]
        public IEnumerable<SelectListItem> GetAllFriends(string id)
        {
            
            var selectList = new List<SelectListItem>();
            var friendsFirst = db.Friends.Where(u => u.User1Id == id).ToList().Select(u => u.User2);
            var friendsSecond = db.Friends.Where(u => u.User2Id == id).ToList().Select(u => u.User1);
            var friends = friendsFirst.Union(friendsSecond);
            foreach (var friend in friends)
            {
                var listItem = new SelectListItem();
                listItem.Value = friend.Email.ToString();
                listItem.Text = friend.Email.ToString();
                selectList.Add(listItem);
            }
            return selectList;
        }*/
 
    }
}