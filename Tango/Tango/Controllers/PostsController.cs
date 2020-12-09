using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tango.Models;

namespace Tango.Controllers
{
    public class PostsController : Controller
    {
        private Tango.Models.AppContext db = new Tango.Models.AppContext();
        // GET: Posts
        public ActionResult Index()
        {
            var posts = db.Posts;
            ViewBag.Posts = posts;
            if(TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }

        public ActionResult Show(int id)
        {
            Post post = db.Posts.Find(id);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View(post);
        }

        public ActionResult New()
        {
            return View();
        }
        [HttpPost]
        public ActionResult NewComment(int id,Comment com)
        {


            try
            {
                if(ModelState.IsValid)
                {
                    com.Date = DateTime.Now;
                    db.Comments.Add(com);
                    db.SaveChanges();
                    TempData["message"] = "Comentariul a fost adaugat!";
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
        [HttpPost]
        public ActionResult New(Post post)
        {
            post.Date = DateTime.Now;
            //post.LastEditDate = null;
            try
            {
                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch(Exception e)
            {
                ViewBag.Error = e;
                return View();
            }
        }
        public ActionResult Edit(int id)
        {
            var post = db.Posts.Find(id);
            return View(post);
        }
        [HttpPut]
        public ActionResult Edit(int id, Post requestpost)
        {

            try
            {

                if(ModelState.IsValid)
                {
                    Post post = db.Posts.Find(id);
                    if(TryValidateModel(post))
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
                    return View(requestpost);
                }



            }catch(Exception e)
            {
                ViewBag.Error = e;
                return View(requestpost);
            }
        }
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            var post = db.Posts.Find(id);
            db.Posts.Remove(post);
            db.SaveChanges();
            TempData["message"] = "Postarea a fost stearsa!";
            return RedirectToAction("Index");

        }




    }
}