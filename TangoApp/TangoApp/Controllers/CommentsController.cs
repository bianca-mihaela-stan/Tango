using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class CommentsController : Controller
    {
        // GET: Comments
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Edit(int id)
        {
            Comment com = db.Comments.Find(id);
            return View(com);
    
        }
        [HttpPut]
        public ActionResult Edit(int id, Comment rcom)
        {

            try
            {
                if(ModelState.IsValid)
                {

                    Comment com = db.Comments.Find(id);
                    if(TryUpdateModel(com))
                    {

                        com.Text = rcom.Text;
                        db.SaveChanges();
                        TempData["message"] = "Comentariul a fost editat!";
                        return Redirect("/Posts/Show/" + com.PostId);
                    }
                    else
                    {
                        return View(rcom);
                    }
         
                }
                else
                {
                    return View(rcom);
                }

       
            }
            catch(Exception e)
            {

                return View(rcom);
            }

        }
        public ActionResult Delete(int id)
        {
            Comment com = db.Comments.Find(id);
            db.Comments.Remove(com);
            db.SaveChanges();
            TempData["message"] = "Comentariul a fost sters!";
            return Redirect("/Posts/Show/" + com.PostId);
        }
    }
}