using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;
using AugPServer.Helpers;
using AugPServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AugPServer.Controllers {
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
                        writer.WriteStartDocument(false);
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
                        if (sessionModel.Figures != null && sessionModel.Figures.Count > 0)
                        {
                            for (int i = 0; i < sessionModel.Figures.Count; i++)
                            {
                                writer.WriteStartElement("Figure");

                                writer.WriteAttributeString("Id", i.ToString());
                                writer.WriteAttributeString("Title", sessionModel.Figures[i].Name);
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

            return this.CheckViewFirst(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectFile(ProjectFileModel model)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if(sessionModel != null)
            {
                sessionModel.ProjectFile = model;
                sessionModel.IsFinished = true;
            }

            this.AddToSession("ProjectInfo", sessionModel);
            return RedirectToAction("NewImages");
        }

        public ActionResult NewImages()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if(sessionModel == null || sessionModel.Figures == null)
                return View(sessionModel);

            //create a directory in the user's session directory
            string pathToSaveNewImages = sessionModel.SessionDirectoryPath + @"\NewImages";
            if (!Directory.Exists(pathToSaveNewImages))
                Directory.CreateDirectory(pathToSaveNewImages);

            HashSet<string> imageNames = new HashSet<string>();
            for (int i = 0; i < sessionModel.Figures.Count; i++)
            {
                ImageModel img = sessionModel.Figures[i].Image;
                int glyphWidth = 0;
                int glyphHeight = 0;

                //set the sizes
                switch (img.GlyphSize)
                {
                    case GlyphSizeChoises.Small:
                        glyphWidth = Consts.GlyphWidthSmall;
                        glyphHeight = Consts.GlyphHeightSmall;
                        break;
                    case GlyphSizeChoises.Medium:
                        glyphWidth = Consts.GlyphWidthMedium;
                        glyphHeight = Consts.GlyphHeightMedium;
                        break;
                    case GlyphSizeChoises.Big:
                        glyphWidth = Consts.GlyphWidthBig;
                        glyphHeight = Consts.GlyphHeightBig;
                        break;
                }
                int qrSize = glyphWidth;

                string QRCode = $"{sessionModel.ProjectFile.URLToFile};{i}"; //the content of the qr code
                using (Image<Rgba32> img_glyph = Image.Load<Rgba32>(_env.ContentRootPath + @"\Glyphs\glyph_0.png")) // the glyph img
                using (Image<Rgba32> img_base = Image.Load<Rgba32>(_env.WebRootPath + img.Path)) // the base image
                using (Image<Rgba32> img_qrCode = Image.Load<Rgba32>(createQRCode(QRCode)))// qr code img
                {
                    int outputWidth = img_base.Width;
                    int outputHeight = img_base.Height;

                    //the glyph and the qr code bigger than the base img? (shouldn't be, but handle it)
                    if (outputWidth < glyphWidth + qrSize)
                        outputWidth = glyphWidth + qrSize;
                    if (outputHeight < glyphHeight + qrSize)
                        outputHeight = glyphHeight + qrSize;

                    //if the glyph is outside we need a bigger img
                    if(img.GlyphOutside)
                        outputWidth += glyphWidth;

                    using (Image<Rgba32> outputImage = new Image<Rgba32>(outputWidth, outputHeight)) // create output image of the correct dimensions
                    {
                        img_glyph.Mutate(o => o.Resize(new Size(glyphWidth, glyphHeight))); //resize glyph
                        img_qrCode.Mutate(o => o.Resize(new Size(qrSize, qrSize))); //resize qrcode

                        int imgX = 0;
                        int imgY = 0;
                        int baseImgOffsetX = 0;
                        switch(img.GlyphPosition)
                        {
                            case GlyphPositionChoises.BottomLeft:
                                imgX = 0;
                                imgY = outputHeight - glyphHeight - qrSize;

                                if(img.GlyphOutside)
                                    baseImgOffsetX = glyphWidth;
                                break;
                            case GlyphPositionChoises.BottomRight:
                                imgX = outputWidth - glyphWidth;
                                imgY = outputHeight - glyphHeight - qrSize;
                                break;
                            case GlyphPositionChoises.TopLeft:
                                imgX = 0;
                                imgY = 0;
                                if (img.GlyphOutside)
                                    baseImgOffsetX = glyphWidth;
                                break;
                            case GlyphPositionChoises.TopRight:
                                imgX = outputWidth - glyphWidth;
                                imgY = 0;
                                break;
                        }

                        //create the new image
                        outputImage.Mutate(o => o
                            .DrawImage(img_base, new Point(baseImgOffsetX, 0), 1f) // base img
                            .DrawImage(img_glyph, new Point(imgX, imgY), 1f) // glyph
                            .DrawImage(img_qrCode, new Point(imgX, imgY + img_glyph.Height), 1f) // qrCode under to the glyph
                        );

                        string imageName = $"{img.Name}";
                        Random rnd = new Random();
                        while (imageNames.Contains(imageName) || !isFileNameOk(imageName)) {
                            imageName = $"output{rnd.Next(0, 1000)}";
                        }
                        imageNames.Add(imageName);
                        outputImage.Save(@$"{pathToSaveNewImages}\{imageName}.png"); //save to the newimages folder
                        sessionModel.Figures[i].FinalImageName = imageName;
                    }
                }
            }

            ZipFile.CreateFromDirectory(pathToSaveNewImages, @$"{sessionModel.SessionDirectoryPath}\output_images.zip");

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

        public ActionResult DownloadImageZIP() {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.IsFinished) {
                string filePath = $@"{sessionModel.SessionDirectoryPath}\output_images.zip";
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/force-download", "output_images.zip");
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

        public ActionResult StartNewProject()
        {
            deleteCurrentProjectDirectory();
            this.RemoveFromSession("ProjectInfo");
            return RedirectToAction("Index", "Page");
        }

        /// <summary>
        /// Delete current project directory
        /// </summary>
        private void deleteCurrentProjectDirectory()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");

            if (Directory.Exists(sessionModel.SessionDirectoryPath))
            {
                {
                    System.Diagnostics.Debug.WriteLine("Remove: " + sessionModel.SessionDirectoryPath);
                    Directory.Delete(sessionModel.SessionDirectoryPath, true); //delete directory
                }
            }
        }

        private bool isFileNameOk(string fileName) {
            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach(char c in fileName) {             
                foreach(char invalidChar in invalidChars) {
                    if(invalidChar == c) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
