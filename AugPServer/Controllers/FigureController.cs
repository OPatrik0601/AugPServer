﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AugPServer.Helpers;
using AugPServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AugPServer.Controllers
{
    public class FigureController : Controller
    {

        public ActionResult FigureList()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            return this.CheckViewFirst((sessionModel.Figures != null) ? sessionModel.Figures : new List<FigureModel>());
        }

        public ActionResult AddFigure()
        {
            FigureModel model = new FigureModel();
            model.ImagePaths = getImagePaths();

            return this.CheckViewFirst(model);
        }

        public ActionResult EditFigure(int id)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.Figures != null)
            {
                if (sessionModel.Figures[id] != null)
                {
                    FigureModel model = sessionModel.Figures[id];
                    model.ImagePaths = getImagePaths();
                    return View(model);
                }
            }

            return View("AddModel");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFigure(int id, FigureModel model)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            model.Image = searchImageByPath(model.ImagePath);
            sessionModel.Figures[id] = model;
            this.AddToSession("ProjectInfo", sessionModel); //save in session
            return RedirectToAction("FigureList");
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

            model.Image = searchImageByPath(model.ImagePath);
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
            if (sessionModel.UploadedImages != null)
            { //there is at least 1 uploaded image
                items = new SelectListItem[sessionModel.UploadedImages.Count + 1]; //the first choice (index 0) is "no image attached", so list length + 1 is the new length
                for (int i = 1; i < sessionModel.UploadedImages.Count + 1; i++)
                {
                    items[i] = new SelectListItem() { Text = sessionModel.UploadedImages[i - 1].Name, Value = sessionModel.UploadedImages[i - 1].Path }; //selection choice
                }
            }
            else
            {
                items = new SelectListItem[1]; //the "no image attached" is the only element
            }

            items[0] = new SelectListItem() { Text = "No image attached.", Value = "Null" };
            return items;
        }

        /// <summary>
        /// Search in the uploaded images for a specific path.
        /// </summary>
        /// <param name="path">The path for search</param>
        /// <returns>The uploaded ImageModel</returns>
        private ImageModel searchImageByPath(string path)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if(sessionModel.UploadedImages != null)
            {
                foreach(ImageModel img in sessionModel.UploadedImages)
                {
                    if (img.Path == path)
                        return img;
                }
            }

            return null;
        }
    }
}
