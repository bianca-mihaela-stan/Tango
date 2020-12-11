using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class ProfileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Profile
        public ActionResult Show()
        {
            var id = User.Identity.GetUserId();
            Profile profile = db.Profiles.Include("User").First(a => a.UserId == id);
            return View(profile);
        }

        public ActionResult Edit()
        {
            Profile pr = db.Profiles.Find(User.Identity.GetUserId());
            return View(pr);
        }

        [HttpPut]
        public ActionResult Edit(Profile NewProfile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Profile pr = db.Profiles.Find(User.Identity.GetUserId());
                    if (TryUpdateModel(pr))
                    {
                        pr.ProfileVisibility = NewProfile.ProfileVisibility;
                        pr.Description = NewProfile.Description;
                        pr.Gender = NewProfile.Gender;
                        TempData["message"] = "Profilul a fost editat!";
                        return Redirect("/Profile/Show/");
                    }
                    else
                    {
                        return View(NewProfile);
                    }
                }
                else
                {
                    return View(NewProfile);
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e;
                return View(NewProfile);
            }
        }
    }
}