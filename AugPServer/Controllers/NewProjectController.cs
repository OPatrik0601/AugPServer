using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AugPServer.Helpers;
using AugPServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;

namespace AugPServer.Controllers
{
    public class NewProjectController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MetaData()
        {
            MetaDataModel sessionModel = this.GetFromSession<MetaDataModel>("MetaDataModel"); //use the given values as metadata if it's already exists in the session (so the user don't have to type it again)
            return View(sessionModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MetaData(MetaDataModel model)
        {
            this.AddToSession("MetaDataModel", model); //add metadata to the session
            return RedirectToAction("ImageUpload");
        }

        public ActionResult ImageUpload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImageUpload(List<IFormFile> files)
        {
            List<string> pathsForImages = new List<string>();
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // getting fileName
                        string fileName = Path.GetFileName(file.FileName);

                        // assigning Unique filename (Guid)
                        string myUniqueFileName = Convert.ToString(Guid.NewGuid());

                        // getting file extension
                        string fileExtension = Path.GetExtension(fileName);

                        // concatenating FileName + FileExtension
                        string newFileName = myUniqueFileName + fileExtension;

                        // combines two strings into a path.
                        string filepath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "TempFiles")).Root + $@"\~{this.SessionId()}\";
                        
                        // create the temp folder if it doesn't exist
                        if (!Directory.Exists(filepath))
                            Directory.CreateDirectory(filepath);

                        filepath += newFileName;

                        string pathToSaveInSession = $@"/TempFiles/~{this.SessionId()}/{newFileName}";

                        using (FileStream fs = System.IO.File.Create(filepath))
                        {
                            file.CopyTo(fs);
                            fs.Flush();

                            pathsForImages.Add(pathToSaveInSession); //add the image paths to the list
                        }

                    }
                }
            }

            List<string> existingImages = this.GetFromSession<List<string>>("ImagePaths");
            if (existingImages != null) //Does the user have an image array in the session already? If that's the case just append the new image list to the old one
            {
                existingImages.AddRange(pathsForImages);
                this.AddToSession("ImagePaths", existingImages);
            }
            else
            {
                this.AddToSession("ImagePaths", pathsForImages);
            }

            return View();
        }

        public ActionResult FigureList()
        {
            List<FigureModel> models = this.GetFromSession<List<FigureModel>>("FigureModel"); //get the figure list from the session
            if (models != null)
                return View(models);
            else
                return View(new List<FigureModel>());
        }

        public ActionResult AddFigure()
        {
            FigureModel model = new FigureModel();
            model.ImagePaths = getImagePaths();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFigure(FigureModel model)
        {
            List<FigureModel> figures = this.GetFromSession<List<FigureModel>>("FigureModel"); //get the figure list from the session
            if (figures == null)
            {
                figures = new List<FigureModel>();
            }
            figures.Add(model); //add the new model to the list
            this.AddToSession("FigureModel", figures); //save in session
            return RedirectToAction("FigureList");
        }

        public ActionResult RemoveFigure(int id)
        {
            List<FigureModel> figures = this.GetFromSession<List<FigureModel>>("FigureModel"); //get the figure list from the session
            if (figures != null)
            {
                figures.RemoveAt(id);
                this.AddToSession("FigureModel", figures);
            }
            return RedirectToAction("FigureList");
        }

        /// <summary>
        /// Get the image paths for the dropdown menu
        /// </summary>
        private IEnumerable<SelectListItem> getImagePaths()
        {
            List<string> pathsForImages = this.GetFromSession<List<string>>("ImagePaths"); //get the image paths from session that the user uploaded
            SelectListItem[] items;
            if (pathsForImages != null) { //there is at least 1 uploaded image
                items = new SelectListItem[pathsForImages.Count+1]; //the first choice (index 0) is "no image attached", so list length + 1 is the new length
                for (int i = 1; i < pathsForImages.Count+1; i++)
                {
                    items[i] = new SelectListItem() { Text = $"uploaded #{i}", Value = pathsForImages[i-1] }; //selection choice
                }
            } else
            {
                items = new SelectListItem[1]; //the "no image attached" is the only element
            }

            items[0] = new SelectListItem() { Text = "No image attached.", Value = "Null" };
            return items;
        }
    }
}
