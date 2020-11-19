using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AugPServer.Helpers;
using AugPServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace AugPServer.Controllers
{
    public class ImageUploadController : Controller
    {
        public ActionResult ImageUpload()
        {
            return this.CheckViewFirst();
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
