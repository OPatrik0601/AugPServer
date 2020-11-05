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
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo"); //use the given values as metadata if it's already exists in the session (so the user don't have to type it again)
            return View((sessionModel != null) ? sessionModel.MetaData : null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MetaData(MetaDataModel model)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel == null) //if the sessionmodel is not exists
            {
                sessionModel = new SessionModelCollector
                {
                    MetaData = model
                };
            }
            else //if exists just update (so the user edited the metadata)
                sessionModel.MetaData = model;

            this.AddToSession("ProjectInfo", sessionModel); //add metadata to the session
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
                        string fileName = Path.GetFileName(file.FileName); // getting fileName
                        string myUniqueFileName = Convert.ToString(Guid.NewGuid()); // assigning Unique filename (Guid)
                        string fileExtension = Path.GetExtension(fileName); // getting file extension
                        string newFileName = myUniqueFileName + fileExtension; // concatenating FileName + FileExtension

                        string filepath = UserDirectoryPath;
                        filepath += newFileName;
                        string pathToSaveInSession = $@"/{"TempFiles"}/~{this.SessionId()}/{newFileName}";

                        using (FileStream fs = System.IO.File.Create(filepath))
                        {
                            file.CopyTo(fs);
                            fs.Flush();

                            pathsForImages.Add(pathToSaveInSession); //add the image path to the list
                        }

                    }
                }
            }

            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.UploadedImagePaths != null) //Does the user have an image array in the session already? If that's the case just append the new image list to the old one
            {
                sessionModel.UploadedImagePaths.AddRange(pathsForImages);
            }
            else
            {
                sessionModel.UploadedImagePaths = pathsForImages;
            }

            this.AddToSession("ProjectInfo", sessionModel);
            return View();
        }

        public ActionResult FigureList()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.Figures != null)
                return View(sessionModel.Figures);
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
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.Figures == null)
            {
                sessionModel.Figures = new List<FigureModel>();
            }

            sessionModel.Figures.Add(model); //add the new model to the list
            this.AddToSession("ProjectInfo", sessionModel); //save in session
            return RedirectToAction("FigureList");
        }

        public ActionResult RemoveFigure(int id)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.Figures != null)
            {
                sessionModel.Figures.RemoveAt(id);
                this.AddToSession("ProjectInfo", sessionModel); //save in session
            }

            return RedirectToAction("FigureList");
        }

        /// <summary>
        /// Get the image paths for the dropdown menu
        /// </summary>
        private IEnumerable<SelectListItem> getImagePaths()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            SelectListItem[] items;
            if (sessionModel.UploadedImagePaths != null) { //there is at least 1 uploaded image
                items = new SelectListItem[sessionModel.UploadedImagePaths.Count+1]; //the first choice (index 0) is "no image attached", so list length + 1 is the new length
                for (int i = 1; i < sessionModel.UploadedImagePaths.Count+1; i++)
                {
                    items[i] = new SelectListItem() { Text = $"uploaded #{i}", Value = sessionModel.UploadedImagePaths[i-1] }; //selection choice
                }
            } else
            {
                items = new SelectListItem[1]; //the "no image attached" is the only element
            }

            items[0] = new SelectListItem() { Text = "No image attached.", Value = "Null" };
            return items;
        }

        private string UserDirectoryPath
        {
            get
            {
                CreateUserDirectoryPathIfNotExists();
                string tempDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "TempFiles");
                return new PhysicalFileProvider(tempDirectoryPath).Root + $@"\~{this.SessionId()}\";
            }
        }

        private void CreateUserDirectoryPathIfNotExists()
        {
            //handle temp files directory
            string tempDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "TempFiles");
            if (!Directory.Exists(tempDirectoryPath))
                Directory.CreateDirectory(tempDirectoryPath);

            //handle the uploaded file path
            string filepath = new PhysicalFileProvider(tempDirectoryPath).Root + $@"\~{this.SessionId()}\";
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
                SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
                sessionModel.SessionDirectoryPath = filepath;
                this.AddToSession("ProjectInfo", sessionModel);
            }
        }
    }
}
