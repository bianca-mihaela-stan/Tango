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


    }
}