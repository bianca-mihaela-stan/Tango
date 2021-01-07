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
            IOrderedQueryable<Profile> profiles = db.Profiles.Include("User").Include("Country").Include("City");
            var search = "";
            var number_of_profiles_perpage = 10;
            if (Request.Params.Get("search") != null)
            {
                //trim whitespace from search string
                search = Request.Params.Get("search").Trim();
                //search in posts (content, name of the creator)
                List<int> postIds = db.Posts.Where(
                    at => at.Text.Contains(search)
                    || at.User.UserName.Contains(search)
                    ).Select(a => a.PostId).ToList();

                profiles = db.Profiles.Where(
                    at => at.User.UserName.Contains(search)).OrderBy(a => a.User.UserName);

                

            }

            var totalItems = profiles.Count();
            var currentPageProfiles = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPageProfiles.Equals(0))
            {
                offset = (currentPageProfiles - 1) * number_of_profiles_perpage;
            }
            var paginatedProfiles = profiles.OrderBy(a => a.User.UserName).Skip(offset).Take(number_of_profiles_perpage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)number_of_profiles_perpage);
            ViewBag.Profiles = paginatedProfiles;
            ViewBag.SearchString = search;

            return View();
        }
        

        [Authorize(Roles ="Editor, User, Admin")]
        public ActionResult Show(string id)
        {
            Profile profile = db.Profiles.First(a => a.UserId == id);
            ViewBag.currentUser = User.Identity.GetUserId();
            return View(profile);
        }

        //show logged user's profile in edit mode
        [Authorize(Roles = "Editor, Admin")]
        public ActionResult Edit(int id)
        {
            Profile Model = new Profile();
            Profile pr = db.Profiles.Find(id);
            if (pr.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                pr.Countries = GetAllCountries();
                pr.Cities = GetAllCities();
                Model = pr;
                return View(Model);
            }
            else
            {
                TempData["message"] = "Nu puteti modifica un profil care nu va apartine!";
                return RedirectToAction("Index");
            }
        }
        
        [HttpGet]
        public JsonResult GetCities(int CountryID)
        {
            var cities = db.Cities.Where(c => c.CountryId.Equals(CountryID)).OrderBy(c => c.CityName).ToList();
            return Json(cities, JsonRequestBehavior.AllowGet);
        }

        //edit logged user's profile
        [HttpPut]
        [Authorize(Roles = "Editor, Admin")]
        public ActionResult Edit(int id, Profile requestProfile)
        {
            requestProfile.Countries = GetAllCountries();
            requestProfile.Cities = GetAllCities();
            Console.WriteLine(requestProfile.Birthday);
            try
            {
                if (ModelState.IsValid)
                {
                    //gasesc profilul cu acest id
                    Profile pr = db.Profiles.Find(id);
                    if (pr.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(pr))
                        {
                            pr.ProfileVisibility = requestProfile.ProfileVisibility;
                            pr.Description = requestProfile.Description;
                            pr.Gender = requestProfile.Gender;
                            pr.CityId = requestProfile.CityId;
                            pr.CountryId = requestProfile.CountryId;
                            pr.Country = db.Countries.Find(pr.CountryId);
                            pr.City = db.Cities.Find(pr.CityId);
                            pr.Birthday = requestProfile.Birthday;
                            db.SaveChanges();
                            TempData["message"] = "Profilul a fost editat!";
                        }
                        return RedirectToAction("Show", "Profile", new { id = pr.UserId });
                    }
                    else
                    {
                        TempData["message"] = "Nu puteti modifica un profil care nu va apartine!";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View(requestProfile);
                }
                 
            }
            catch (Exception e)
            {
                ViewBag.Err = e;
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
           

            // returnam lista de categorii
            return selectList;
        }
    }
}


