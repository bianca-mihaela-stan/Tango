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
            var profiles = db.Profiles.Include("Country").Include("User").Include("City");
            ViewBag.profiles = profiles;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }
        

        //show logged user's profile
        [Authorize(Roles ="Editor, User, Admin")]
        public ActionResult Show()
        {
            var id = User.Identity.GetUserId();
            Profile profile = db.Profiles.Include("User").First(a => a.UserId == id);
            return View(profile);
        }


        //show logged user's profile in edit mode
        [Authorize(Roles = "Editor, User, Admin")]
        public ActionResult Edit()
        {
            String id = User.Identity.GetUserId();
            Profile pr = db.Profiles.Include("User").First(a => a.UserId == id);
            pr.Countries = GetAllCountries();
            pr.Cities = GetAllCities();
            return View(pr);
        }

        //edit logged user's profile
        [HttpPut]
        [Authorize(Roles = "Editor, User, Admin")]
        public ActionResult Edit(Profile requestProfile)
        {
            requestProfile.Countries = GetAllCountries();
            requestProfile.Cities = GetAllCities();
            try
            {
                if (ModelState.IsValid)
                {
                    Profile pr = db.Profiles.Find(User.Identity.GetUserId());
                    if (TryUpdateModel(pr))
                    {
                        pr.ProfileVisibility = requestProfile.ProfileVisibility;
                        pr.Description = requestProfile.Description;
                        pr.Gender = requestProfile.Gender;
                        pr.Country = requestProfile.Country;
                        pr.CountryId = requestProfile.CountryId;
                        pr.UserId = User.Identity.GetUserId();
                        pr.User = db.Users.Find(pr.UserId);
                        pr.Birthday = requestProfile.Birthday;
                        pr.City = requestProfile.City;
                        pr.CityId = requestProfile.CityId;
                        db.SaveChanges();
                        TempData["message"] = "Profilul a fost editat!";
                    }
                    return Redirect("/Profile/Show/");
                }
                else
                {
                    return View(requestProfile);
                }
            }
            catch (Exception e)
            {
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

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCities()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var cities = from cat in db.Cities
                         select cat;

            // iteram prin categorii
            foreach (var city in cities)
            {
                // adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = city.CityId.ToString(),
                    Text = city.CityName.ToString()
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


