using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tango.Models;

namespace Tango.Controllers
{
    public class MediaController : Controller
    {
        // GET: Media
        private Tango.Models.AppContext db = new Tango.Models.AppContext();

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase uploadedMedia)
        {
            string UploadedMediaName = uploadedMedia.FileName;
            string UploadedMediaExtension = Path.GetExtension(UploadedMediaName);
            if(UploadedMediaExtension == ".png" || UploadedMediaExtension == ".jpg")
            {
                string UploadFolderPath = Server.MapPath("~//Files//");
                uploadedMedia.SaveAs(UploadFolderPath + UploadedMediaName);
                Media media = new Media();
                media.MediaExtension = UploadedMediaExtension;
                media.MediaName = UploadedMediaName;
                media.MediaPath = UploadFolderPath + UploadedMediaName;
                db.Media.Add(media);
                db.SaveChanges();
                TempData["message"] = "Postarea a fost adaugata cu succes!";
                return Redirect("/Posts/Index");
            }
            return View();
        }
    }
}