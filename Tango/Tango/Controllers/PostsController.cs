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
            return View();
        }

        public ActionResult Show(int id)
        {
            Post post = db.Posts.Find(id);

            return View(post);
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Post post)
        {
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




    }
}