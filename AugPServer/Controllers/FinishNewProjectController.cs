using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AugPServer.Helpers;
using AugPServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace AugPServer.Controllers
{
    public class FinishNewProjectController : Controller
    {
        public ActionResult ProjectFile()
        {
            ProjectFileModel model = new ProjectFileModel()
            {
                PathToFile = "ERROR"
            };

            try
            {
                SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
                var filepath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "TempFiles")).Root + $@"\~{this.SessionId()}\document.augp";
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
    }
}
