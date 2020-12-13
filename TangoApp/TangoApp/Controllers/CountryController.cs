using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TangoApp.Models;

namespace TangoApp.Controllers
{
    public class CountryController : Controller
    {
        // GET: Country
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            if(TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            var countries = from country in db.Countries
                            orderby country.CountryName
                            select country;
            ViewBag.Countries = countries;
            return View();
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Country cat)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Countries.Add(cat);
                    db.SaveChanges();
                    TempData["message"] = "Tara a fost adaugata!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(cat);
                }
            }
            catch (Exception e)
            {
                return View(cat);
            }
        }
    }
}