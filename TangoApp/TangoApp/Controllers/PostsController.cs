﻿using Microsoft.AspNet.Identity;
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
        [Authorize(Roles ="User,Editor,Admin")]
        public ActionResult Index()
        {
            IOrderedQueryable<Post> posts = db.Posts.Include("User");
            var search = "";

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
                
            }

            var totalItems = posts.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if(!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * 10;
            }
            var paginatedPosts = posts.OrderBy(a => a.Date).Skip(offset).Take(10);

            if(TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)10);
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
        public ActionResult Edit(int id, Post requestpost, HttpPostedFileBase uploadedMedia)
        {
            if (TempData.ContainsKey("message"))
                ViewBag.Message = TempData["message"];
           
            try
            {

                if(ModelState.IsValid)
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
                                changeMedia(post.PostId, uploadedMedia);

                            ///if we already have an image in db, we have to remove it 
                            /*if (post.Media!= null && post.Media.ToList().Count() == 1)
                            {

                                var media = post.Media.ToList().First();
                                db.Media.Remove(media);
                            }*/
                            db.SaveChanges();
                            TempData["message"] = "Postarea a fost editata!";
                            return Redirect("/Posts/Show/" + id);

                        }
                        else
                        {
                            TempData["message"] = "Not right!";
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
                    TempData["message"] = "Something went wrong!";
                    return View(requestpost);
                }



            }catch(Exception e)
            {
                TempData["message"] = e;
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
                    throw new Exception("Tipul fisierului nu e valid");
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }
        [NonAction]
        public void changeMedia(int postId, HttpPostedFileBase uploadedMedia)
        {
            try
            {

                var post = db.Posts.Find(postId);
                var mediaToUpdate = post.Media.ToList().First();

                ///either way, we will upload in db a new file
                /* int mediaId = Upload(uploadedMedia);
                 var c = 4;
                 var image = db.Media.Find(mediaId);
                 image.PostId = postId;*/
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