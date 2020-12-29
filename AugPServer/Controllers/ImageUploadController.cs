using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AugPServer.Helpers;
using AugPServer.Models;
using AugPServer.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace AugPServer.Controllers
{
    public class ImageUploadController : Controller
    {
        public ActionResult ImageUpload()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            return this.CheckViewFirst(sessionModel.UploadedImages);
        }

        public ActionResult EditAllImages()
        {
            return this.CheckViewFirst();
        }

        public ActionResult EditImage(int id)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.UploadedImages != null)
            {
                if (sessionModel.UploadedImages[id] != null)
                {
                    ImageModel model = sessionModel.UploadedImages[id];
                    return this.CheckViewFirst(model);
                }
            }

            return View("AddModel");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditImage(int id, ImageModel model)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");

            model.Path = sessionModel.UploadedImages[id].Path; //don't change the path
            sessionModel.UploadedImages[id] = model;
            this.AddToSession("ProjectInfo", sessionModel); //save in session
            return RedirectToAction("ImageList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAllImages(EditAllImagesViewModel model)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.UploadedImages != null)
            {
                foreach(ImageModel img in sessionModel.UploadedImages)
                {
                    if(model.GlyphOutside != null)
                        img.GlyphOutside = (bool)model.GlyphOutside;

                    if(model.GlyphPosition != null)
                        img.GlyphPosition = (GlyphPositionChoises)model.GlyphPosition;

                    if(model.GlyphSize != null)
                        img.GlyphSize = (GlyphSizeChoises)model.GlyphSize;
                }
                this.AddToSession("ProjectInfo", sessionModel); //save in session
            }
            return RedirectToAction("ImageList");
        }

        public ActionResult ImageList()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");

            return this.CheckViewFirst((sessionModel.UploadedImages != null) ? sessionModel.UploadedImages : new List<ImageModel>());
        }

        [HttpPost]
        public ActionResult ImageUpload(List<IFormFile> files)
        {
            List<ImageModel> pathsForImages = new List<ImageModel>();
            if (files != null)
            {
                for (int i=0;i<files.Count;i++)
                {
                    IFormFile file = files[i];
                    if (!UploadedFileIsImage.Check(file))
                        continue;

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

                            ImageModel newModel = new ImageModel()
                            {
                                Path = pathToSaveInSession,
                                Name = $"img{i}:{DateTime.Now}",
                                GlyphSize = GlyphSizeChoises.Medium,
                                GlyphOutside = false,
                                GlyphPosition = GlyphPositionChoises.TopLeft
                            };

                            pathsForImages.Add(newModel); //add the image path to the list
                        }
                    }
                }
            }

            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.UploadedImages != null) //Does the user have an image array in the session already? If that's the case just append the new image list to the old one
            {
                sessionModel.UploadedImages.AddRange(pathsForImages);
            }
            else
            {
                sessionModel.UploadedImages = pathsForImages;
            }

            this.AddToSession("ProjectInfo", sessionModel);
            return View(sessionModel.UploadedImages);
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
