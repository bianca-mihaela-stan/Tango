using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Posts
        [Authorize(Roles ="User,Editor,Admin")]
        public ActionResult Index()
        {
            IOrderedQueryable<Post> posts = db.Posts.Include("User");
            IOrderedQueryable<Profile> profiles = db.Profiles.Include("User").Include("Country").Include("City");
            var search = "";
            var number_of_posts_perpage = 10;
            if(Request.Params.Get("search")!=null)
            {
                //trim whitespace from search string
                search = Request.Params.Get("search").Trim();
                //search in posts (content, name of the creator)
                List<int> postIds = db.Posts.Where(
                    at => at.Text.Contains(search)
                    || at.User.UserName.Contains(search)
                    ).Select(a => a.PostId).ToList();

                //search in comments
                List<int> commentIds = db.Comments.Where(c => c.Text.Contains(search)).Select(com => com.PostId).ToList();

                //unique list of articles
                List<int> mergedIds = postIds.Union(commentIds).ToList();
                posts = db.Posts.Where(post => mergedIds.Contains(post.PostId)).Include("User").OrderBy(a => a.Date).OrderBy(a => a.Date);

                profiles = db.Profiles.Where(
                    at => at.User.UserName.Contains(search)).OrderBy(a => a.User.UserName);
                
            }

            var totalItems = posts.Count();
            var currentPagePosts = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if(!currentPagePosts.Equals(0))
            {
                offset = (currentPagePosts - 1) * number_of_posts_perpage;
            }
            var paginatedPosts = posts.OrderBy(a => a.Date).Skip(offset).Take(number_of_posts_perpage);

            if(TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)number_of_posts_perpage);
            ViewBag.Posts = paginatedPosts;
            ViewBag.SearchString = search;
            

            return View();
        }
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Show(int id)
        {
            Post post = db.Posts.Find(id);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View(post);
        }
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult New()
        {
            Post post = new Post();
            post.UserId = User.Identity.GetUserId();
            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult New(Post post)
        {
           
            try
            {
                if (ModelState.IsValid)
                {
                    post.Date = DateTime.Now;
                    post.UserId = User.Identity.GetUserId();
                    db.Posts.Add(post);
                    db.SaveChanges();
                    TempData["message"] = "Postarea a fost adaugata!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View();
                }
            }
            catch(Exception e)
            {
                ViewBag.Error = e;
                return View();
            }
        }
        [HttpPost]
        public ActionResult NewComment(int id, Comment com)
        {


            try
            {
                if (ModelState.IsValid)
                {
                    com.Date = DateTime.Now;
                    com.UserId = User.Identity.GetUserId();
                    db.Comments.Add(com);
                    db.SaveChanges();
                    TempData["message"] = "Comentariul a fost adaugat!";
                    //adaugam notificarea corespunzatoare
                    try
                    {
                        Notification notification = new Notification();
                        notification.UserSendId = User.Identity.GetUserId();
                        var post = db.Posts.Find(id);
                        notification.UserReceiveId = post.UserId;
                        notification.PostId = id;
                        notification.CommentId = com.CommentId;
                        notification.Time = DateTime.Now;
                        notification.Seen = false;
                        notification.Type = NotificationFlag.NewComment;
                        db.Notifications.Add(notification);
                        db.SaveChanges();
                    }
                    catch(Exception e)
                    {
                        TempData["message"] = " Eroare la adaugarea notificarii" + e;

                    }


                    return Redirect("/Posts/Show/" + com.PostId);
                }
                else
                {
                    Post post = db.Posts.Find(id);
                    ViewBag.Comment = com;
                    return View("Show", post);

                }

            }
            catch (Exception e)
            {

                Post post = db.Posts.Find(id);
                return View("Show", post);
            }


        }
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Edit(int id)
        {
            var post = db.Posts.Find(id);
            if(post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                return View(post);
            else
            {
                TempData["message"] = "Nu aveti permisiunea de a edita aceasta postare!";
                return RedirectToAction("Index");
            }
        }
        [HttpPut]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Edit(int id, Post requestpost)
        {

            try
            {

                if(ModelState.IsValid)
                {
                    var post = db.Posts.Find(id);
                    if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryValidateModel(post))
                        {

                            post.Text = requestpost.Text;
                            post.LastEditDate = DateTime.Now;
                            db.SaveChanges();
                            TempData["message"] = "Postarea a fost editata!";
                            return Redirect("/Posts/Show/" + id);

                        }
                        else
                        {
                            return View(requestpost);

                        }
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti permisiunea de a edita aceasta postare!";
                        return RedirectToAction("Index");
                    }


                }
                else
                {
                    return View(requestpost);
                }



            }catch(Exception e)
            {
                ViewBag.Error = e;
                return View(requestpost);
            }
        }
        [HttpDelete]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Delete(int id)
        {
            var post = db.Posts.Find(id);
            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Posts.Remove(post);
                var notificationList = db.Notifications.Where(u => u.PostId == post.PostId).ToList();
                if (notificationList.Any())
                {
                    var notification = notificationList.First();
                    notification.PostId = null;
                    notification.CommentId = null;
                }
               
                TempData["message"] = "Postarea a fost stearsa!";
                ///daca continutul a fost gasit necorespunzator si a fost sters de admin, atunci
                // trebuie sa adaugam o notificare corespunzatoare pentru User-ul care a postat comentariul
                if(User.IsInRole("Admin"))
                {
                    Notification notification = new Notification();
                    notification.UserSendId = User.Identity.GetUserId();
                    notification.Time = DateTime.Now;
                    notification.Seen = false;
                    notification.Type = NotificationFlag.DeletedPost;
                    db.Notifications.Add(notification);
                   
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti permisiunea de a sterge aceasta postare!";
                return RedirectToAction("Index");
            }

        }




    }
}