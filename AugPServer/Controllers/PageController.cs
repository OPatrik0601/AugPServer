using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AugPServer.Helpers;
using AugPServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;

namespace AugPServer.Controllers
{
    public class PageController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Downloads()
        {
            return View();
        }

        public ActionResult DownloadFileFormDownloads(string fileNameToDownload) {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads", fileNameToDownload);

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            return File(fileBytes, "application/force-download", fileNameToDownload);
        }
    }
}
