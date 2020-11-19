using System;
using System.IO;
using System.Xml;
using AugPServer.Helpers;
using AugPServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AugPServer.Controllers
{
    public class FinishNewProjectController : Controller
    {
        private readonly IWebHostEnvironment _env;

        private const int glyphWidth = 53;
        private const int glyphHeight = 76;
        private const int qrSize = 38;

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

                        if (sessionModel.Authors != null)
                        {
                            foreach(var item in sessionModel.Authors)
                            {
                                writer.WriteStartElement("Author");

                                writer.WriteAttributeString("Name", item.FullName);
                                writer.WriteAttributeString("Affiliation", item.Affiliation);

                                writer.WriteEndElement(); //</Figure>
                            }
                        }
                        else
                        {
                            writer.WriteComment("No authors found.");
                        }

                        writer.WriteEndElement(); //</Authors>

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
                sessionModel.ProjectFile = model;
                this.AddToSession("ProjectInfo", sessionModel);

            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectFile(ProjectFileModel model)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if(sessionModel != null)
            {
                sessionModel.ProjectFile = model;
            }

            this.AddToSession("ProjectInfo", sessionModel); //add metadata to the session
            return RedirectToAction("NewImages");
        }

        public ActionResult NewImages()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");

            //create a directory in the user's session directory
            string pathToSaveNewImages = sessionModel.SessionDirectoryPath + @"\NewImages";
            if (!Directory.Exists(pathToSaveNewImages))
                Directory.CreateDirectory(pathToSaveNewImages);

            for (int i = 0; i < sessionModel.Figures.Count; i++)
            {
                string QRCode = $"{sessionModel.ProjectFile.URLToFile};{i}"; //the content of the qr code
                using (Image<Rgba32> img_glyph = Image.Load<Rgba32>(_env.ContentRootPath + @"\Glyphs\glyph_0.png")) // the glyph img
                using (Image<Rgba32> img_base = Image.Load<Rgba32>(_env.WebRootPath + sessionModel.UploadedImagePaths[i])) // the base image
                using (Image<Rgba32> img_qrCode = Image.Load<Rgba32>(createQRCode(QRCode))) // qr code img
                using (Image<Rgba32> outputImage = new Image<Rgba32>(img_base.Width, img_base.Height)) // create output image of the correct dimensions
                {
                    img_glyph.Mutate(o => o.Resize(new Size(glyphWidth, glyphHeight))); //resize glyph
                    img_qrCode.Mutate(o => o.Resize(new Size(qrSize, qrSize))); //resize qrcode

                    //create the new image
                    outputImage.Mutate(o => o
                        .DrawImage(img_base, new Point(0, 0), 1f) // base img
                        .DrawImage(img_glyph, new Point(0, 0), 1f) // glyph
                        .DrawImage(img_qrCode, new Point(img_glyph.Width, 0), 1f) // qrCode next to the glyph
                    );

                    outputImage.Save(@$"{pathToSaveNewImages}\output_{i}.png"); //save to the newimages folder
                }
            }

            return View(sessionModel);
        }

        public ActionResult DownloadAugpFile()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.ProjectFile != null) {
                string filePath = sessionModel.ProjectFile.PathToFile;
                string fileName = "document.augp";

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                return File(fileBytes, "application/force-download", fileName);
            } else
                return View();
        }

        private byte[] createQRCode(string code)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20);

            using (MemoryStream stream = new MemoryStream())
            {
                qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
