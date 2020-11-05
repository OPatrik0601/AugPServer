using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AugPServer.Helpers;
using AugPServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AugPServer.Controllers
{
    public class FinishNewProjectController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public FinishNewProjectController(IWebHostEnvironment _environment)
        {
            _env = _environment;
        }

        public ActionResult ProjectFile()
        {
            ProjectFileModel model = new ProjectFileModel()
            {
                PathToFile = "ERROR"
            };

            try
            {
                SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
                var filepath = sessionModel.SessionDirectoryPath + @"\document.augp";
                using (FileStream fileStream = new FileStream(filepath, FileMode.Create)) //create the .augp file in the user's temp folder
                {
                    XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
                    using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
                    {
                        writer.WriteStartDocument(true);
                        //start file
                        writer.WriteStartElement("AugmentedPaper");

                        //METADATA
                        writer.WriteStartElement("MetaData");
                        writer.WriteAttributeString("ArticleName", sessionModel.MetaData.ProjectName);
                        writer.WriteAttributeString("DOI", sessionModel.MetaData.DOI);

                        writer.WriteStartElement("Authors");
                        //writer.WriteElementString("..", "1");
                        //later when the user can add multiple authors
                        writer.WriteEndElement();

                        writer.WriteEndElement(); //</MetaData>
                        //METADATA ENDS

                        //FIGURES
                        writer.WriteStartElement("Figures");

                        //Figures
                        if (sessionModel.Figures != null)
                        {
                            for (int i = 0; i < sessionModel.Figures.Count; i++)
                            {
                                writer.WriteStartElement("Figure");

                                writer.WriteAttributeString("Id", i.ToString());
                                writer.WriteAttributeString("ObjFile", sessionModel.Figures[i].ObjPath);
                                writer.WriteAttributeString("MtlFile", sessionModel.Figures[i].MtlPath);

                                writer.WriteEndElement(); //</Figure>
                            }
                        } else
                        {
                            writer.WriteComment("No models found.");
                        }

                        writer.WriteEndElement(); //</Figures>
                        //FIGURES END

                        writer.WriteEndElement(); //</AugmentedPaper>
                        //end file

                        writer.Flush();
                    }
                    fileStream.Flush();
                }

                model.PathToFile = filepath;
                this.AddToSession("ProjectFile", model);

            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }

            return View(model);
        }

        public ActionResult NewImages()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            string pathToSaveNewImages = sessionModel.SessionDirectoryPath + @"\NewImages";
            if (!Directory.Exists(pathToSaveNewImages))
                Directory.CreateDirectory(pathToSaveNewImages);

            for (int i = 0; i < sessionModel.UploadedImagePaths.Count; i++)
            {
                using (Image<Rgba32> img1 = Image.Load<Rgba32>(_env.ContentRootPath + @"\Glyphs\glyph_0.png")) // the glyph img
                using (Image<Rgba32> img2 = Image.Load<Rgba32>(_env.WebRootPath + sessionModel.UploadedImagePaths[i])) // the base image
                using (Image<Rgba32> outputImage = new Image<Rgba32>(img2.Width, img2.Height)) // create output image of the correct dimensions
                {
                    img1.Mutate(o => o.Resize(new Size(100, 150))); //resize glyph

                    outputImage.Mutate(o => o
                        .DrawImage(img2, new Point(0, 0), 1f) // base img
                        .DrawImage(img1, new Point(0, 0), 1f) // glyph
                    );

                    outputImage.Save(@$"{pathToSaveNewImages}\ouput_{i}.png"); //save to the newimages folder
                }
            }

            return View();
        }

        public ActionResult DownloadAugpFile()
        {
            ProjectFileModel pfm = this.GetFromSession<ProjectFileModel>("ProjectFile");
            if (pfm != null) {
                string filePath = pfm.PathToFile;
                string fileName = "document.augp";

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                return File(fileBytes, "application/force-download", fileName);
            } else
                return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectFile(ProjectFileModel model)
        {
            return RedirectToAction("NewImages");
        }
    }
}
