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
                var filepath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "TempFiles")).Root + $@"\~{this.SessionId()}\document.augp";
                using (FileStream fileStream = new FileStream(filepath, FileMode.Create)) //create the .augp file in the user's temp folder
                {
                    XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
                    using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
                    {
                        writer.WriteStartDocument();

                        writer.WriteStartElement("MetaData");

                        writer.WriteEndElement();

                        writer.WriteEndDocument();
                    }
                }

                model.PathToFile = filepath;
                this.AddToSession("ProjectFile", model);

            } catch(Exception ex)
            {
                //...
            }

            return View(model);
        }
    }
}
