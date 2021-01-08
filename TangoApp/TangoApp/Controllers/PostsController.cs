using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
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
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Index()
        {
            var user = db.Profiles.Where(a => a.ProfileVisibility == true).ToList().Select(a =>a.UserId);
            List<Post> posts = db.Posts.Include("User").Where(a => user.Contains(a.UserId)).ToList();
            //IOrderedQueryable<Post> posts = db.Posts.Include("User");
            IOrderedQueryable<Profile> profiles = db.Profiles.Include("User").Include("Country").Include("City");
            var search = "";
            var number_of_posts_perpage = 10;
            if (Request.Params.Get("search") != null)
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
                posts = db.Posts.Where(post => mergedIds.Contains(post.PostId) && user.Contains(post.UserId)).Include("User").OrderBy(a => a.Date).OrderBy(a => a.Date).ToList();

                profiles = db.Profiles.Where(
                    at => at.User.UserName.Contains(search)).OrderBy(a => a.User.UserName);

            }

            var totalItems = posts.Count();
            var currentPagePosts = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPagePosts.Equals(0))
            {
                offset = (currentPagePosts - 1) * number_of_posts_perpage;
            }
            var paginatedPosts = posts.OrderBy(a => a.Date).Skip(offset).Take(number_of_posts_perpage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)number_of_posts_perpage);
            ViewBag.Posts = paginatedPosts;
            ViewBag.SearchString = search;
            ViewBag.esteAdmin = User.IsInRole("Admin");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();


            return View();
        }
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult NewsFeed()
        {
            var currentUser = User.Identity.GetUserId();
            var friendsFirst = db.Friends.Where(u => u.User1Id == currentUser).ToList().Select(u => u.User2Id);
            var friendsSecond = db.Friends.Where(u => u.User2Id == currentUser).ToList().Select(u => u.User1Id);
            var allfriends = friendsFirst.Union(friendsSecond);

            ViewBag.Creator = User.IsInRole("Editor");
            var posts = db.Posts.Where(u => allfriends.Contains(u.UserId) || u.UserId==currentUser).ToList().OrderByDescending(u => (u.Comments.ToList().Any() == false ? u.Date : (u.Date >  u.Comments.ToList().OrderByDescending(i => i.Date).First().Date ? u.Date : u.Comments.ToList().OrderByDescending(i => i.Date).First().Date)));
            ViewBag.Posts = posts;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
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
            ViewBag.afisareButoane = false;
            if (User.IsInRole("Editor") || User.IsInRole("Admin"))
            {
                ViewBag.afisareButoane = true;
            }
            ViewBag.esteAdmin = User.IsInRole("Admin");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
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
        public ActionResult New(Post post, HttpPostedFileBase uploadedMedia)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    post.Date = DateTime.Now;
                    post.UserId = User.Identity.GetUserId();
                    db.Posts.Add(post);
                    if (uploadedMedia != null)
                    {
                        
                            int mediaId = Upload(uploadedMedia);
                            var image = db.Media.Find(mediaId);
                            image.PostId = post.PostId;
                        
                    }
                    db.SaveChanges();
                    TempData["message"] = "Postarea a fost adaugata!";
                    return RedirectToAction("Newsfeed");
                }
                else
                {
                    //ViewBag.Message = "Data invalide";
                    return View(post);
                }
            }
            catch (Exception e)
            {
                //ViewBag.Mesage = e.Message;
                return View(post);
            }
        }
        [HttpPost]
        public ActionResult NewComment(int id, Comment com)
        {
            ViewBag.esteAdmin = User.IsInRole("Admin");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();

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
                        var post = db.Posts.Find(id);
                        //if a user comments on his/her own post, we won't send a notification
                        if(post.UserId != User.Identity.GetUserId())
                        {
                            Notification notification = new Notification();
                            notification.UserSendId = User.Identity.GetUserId();
                            notification.UserReceiveId = post.UserId;
                            notification.PostId = id;
                            notification.CommentId = com.CommentId;
                            notification.Time = DateTime.Now;
                            notification.Seen = false;
                            notification.Type = NotificationFlag.NewComment;
                            db.Notifications.Add(notification);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception e)
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
            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                return View(post);
            else
            {
                TempData["message"] = "Nu aveti permisiunea de a edita aceasta postare!";
                return RedirectToAction("Index");
            }
        }
        [HttpPut]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Edit(int id, Post requestpost, HttpPostedFileBase uploadedMedia)
        {
            if (TempData.ContainsKey("message"))
                ViewBag.Message = TempData["message"];

            try
            {

                if (ModelState.IsValid)
                {
                    var post = db.Posts.Find(id);
                    if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(post))
                        {

                            post.Text = requestpost.Text;
                            post.LastEditDate = DateTime.Now;

                            //we also change the photo if we've got a new one
                            if (uploadedMedia != null)
                            {
                                if (post.Media != null && post.Media.ToList().Any())
                                    changeMedia(post.PostId, uploadedMedia);
                                else
                                {
                                    //if we don't have one, we need to add a new photo
                                    int mediaId = Upload(uploadedMedia);
                                    var image = db.Media.Find(mediaId);
                                    image.PostId = post.PostId;
                                }
                            }
                            //we notify the user if the admin was the one that edited the post
                            if (User.IsInRole("Admin") &&  post.UserId != User.Identity.GetUserId())
                            {
                                Notification notification = new Notification();
                                notification.UserSendId = User.Identity.GetUserId();
                                notification.UserReceiveId = post.UserId;
                                notification.PostId = post.PostId;
                                notification.Time = DateTime.Now;
                                notification.Seen = false;
                                notification.Type = NotificationFlag.EditedPost;
                                db.Notifications.Add(notification);

                            }

                            db.SaveChanges();
                            TempData["message"] = "Postarea a fost editata!";
                            return Redirect("/Posts/Show/" + id);

                        }
                        else
                        {
                            //TempData["message"] = "Not right!";
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
                    //TempData["message"] = "Something went wrong!";
                    return View(requestpost);
                }



            }
            catch (Exception e)
            {
               
                //ViewBag.Error = e;
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
                
                ///remove any notification that references the deleted post
                var notificationList = db.Notifications.Where(u => u.PostId == post.PostId).ToList();
                if (notificationList.Any())
                {
                    foreach (var notification in notificationList)
                    {
                        db.Notifications.Remove(notification);
                    }
                }
                ///remove any media that references the deleted post
                var mediaUsed = post.Media;
                if (mediaUsed != null && mediaUsed.ToList().Any())
                {
                    foreach (var med in mediaUsed.ToList())
                    {
                        db.Media.Remove(med);
                    }

                }
               
                ///daca continutul a fost gasit necorespunzator si a fost sters de admin, atunci
                // trebuie sa adaugam o notificare corespunzatoare pentru User-ul care a postat comentariul
                if (User.IsInRole("Admin") && post.UserId != User.Identity.GetUserId())
                {
                    Notification notification = new Notification();
                    notification.UserSendId = User.Identity.GetUserId();
                    notification.UserReceiveId = post.UserId;
                    notification.Time = DateTime.Now;
                    notification.Seen = false;
                    notification.Type = NotificationFlag.DeletedPost;
                    db.Notifications.Add(notification);

                }
                db.Posts.Remove(post);
                TempData["message"] = "Postarea a fost stearsa!";
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti permisiunea de a sterge aceasta postare!";
                return RedirectToAction("Index");
            }

        }
        [NonAction]
        public int Upload(HttpPostedFileBase uploadedMedia)
        {
            try
            {

                string uploadedMediaName = uploadedMedia.FileName;
                string uploadedMediaExtension = Path.GetExtension(uploadedMediaName);
                if (uploadedMediaExtension == ".png" || uploadedMediaExtension == ".jpg" || uploadedMediaExtension == ".jpeg")
                {

                    string uploadedFolderPath = Server.MapPath("~//Files//");
                    uploadedMedia.SaveAs(uploadedFolderPath + uploadedMediaName);

                    System.Diagnostics.Debug.WriteLine(uploadedFolderPath);
                    Media media = new Media
                    {
                        Extension = uploadedMediaExtension,
                        FileName = uploadedMediaName,
                        FilePath = uploadedFolderPath + uploadedMediaName
                    };
                    db.Media.Add(media);

                    return media.FileId;

                }
                else
                {
                    ViewBag.Message = "Tipul fisierului nu e valid!";
                    throw new Exception("Tipul fisierului nu e valid");
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        [NonAction]
        public void changeMedia(int postId, HttpPostedFileBase uploadedMedia)
        {
            try
            {

                var post = db.Posts.Find(postId);
                var mediaToUpdate = post.Media.ToList().First();

             
                if (TryUpdateModel(mediaToUpdate))
                {
                    mediaToUpdate.FileName = uploadedMedia.FileName;
                    string uploadedMediaExtension = Path.GetExtension(uploadedMedia.FileName);
                    if (uploadedMediaExtension == ".png" || uploadedMediaExtension == ".jpg" || uploadedMediaExtension == ".jpeg")
                    {

                        string uploadedFolderPath = Server.MapPath("~//Files//");
                        uploadedMedia.SaveAs(uploadedFolderPath + uploadedMedia.FileName);


                        mediaToUpdate.Extension = uploadedMediaExtension;
                        mediaToUpdate.FilePath = uploadedFolderPath + uploadedMedia.FileName;


                    }
                    else
                    {
                        ViewBag.Message = "Tipul fisierului nu e valid!";
                        throw new Exception("Tipul fisierului nu e valid");
                    }
                }
                else
                {

                    throw new Exception("Fisierul nu poate fi modificat!");

                }


            }
            catch (Exception e)
            {
                throw;
            }





        }

    }
}