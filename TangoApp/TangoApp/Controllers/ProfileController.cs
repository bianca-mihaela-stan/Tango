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
        public ActionResult Index()
        {
            var profiles = db.Profiles.Include("Country").Include("User");
            ViewBag.profiles = profiles;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }
        [Authorize(Roles ="Editor, User, Admin")]
        public ActionResult Show()
        {
            var id = User.Identity.GetUserId();
            Profile profile = db.Profiles.Include("User").First(a => a.UserId == id);
            return View(profile);
        }

        [HttpPost]
        public ActionResult New()
        {
            Profile profile = new Profile();
            profile.ProfileVisibility = true;
            profile.UserId = User.Identity.GetUserId();
            db.Profiles.Add(profile);
            db.SaveChanges();
            return Redirect("/Home/Index/");
        }

        [Authorize(Roles = "Editor, User, Admin")]
        public ActionResult Edit()
        {
            String id = User.Identity.GetUserId();
            Profile pr = db.Profiles.Include("User").First(a => a.UserId == id);
            pr.Countries = GetAllCountries();
            return View(pr);
        }

        [HttpPut]
        [Authorize(Roles = "Editor, User, Admin")]
        public ActionResult Edit(Profile requestProfile)
        {
            requestProfile.Countries = GetAllCountries();
            try
            {
                if (ModelState.IsValid)
                {
                    Profile pr = db.Profiles.Find(User.Identity.GetUserId());
                    ViewBag.Message = User.Identity.GetUserId();
                    if (TryUpdateModel(pr))
                    {
                        pr.ProfileVisibility = requestProfile.ProfileVisibility;
                        pr.Description = requestProfile.Description;
                        pr.Gender = requestProfile.Gender;
                        TempData["message"] = "Profilul a fost editat!";
                        return Redirect("/Profile/Show/");
                    }
                    else
                    {
                        return View(requestProfile);
                    }
                }
                else
                {
                    return View(requestProfile);
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e;
                return View(requestProfile);
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCountries()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var countries= from cat in db.Countries
                             select cat;

            // iteram prin categorii
            foreach (var country in countries)
            {
                // adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = country.CoutryId.ToString(),
                    Text = country.CountryName.ToString()
                });
            }
            /*
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.CategoryId.ToString();
                listItem.Text = category.CategoryName.ToString();

                selectList.Add(listItem);
            }*/

            // returnam lista de categorii
            return selectList;
        }
    }
}


