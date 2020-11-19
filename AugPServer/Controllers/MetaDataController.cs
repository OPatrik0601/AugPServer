using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AugPServer.Helpers;
using AugPServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AugPServer.Controllers
{
    public class MetaDataController : Controller
    {
        public ActionResult MetaData()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo"); //use the given values as metadata if it's already exists in the session (so the user don't have to type it again)
            return this.CheckViewFirst((sessionModel != null) ? sessionModel.MetaData : null);
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
                    MetaData = model,
                    SessionId = this.SessionId()
                };
            }
            else //if exists just update (so the user edited the metadata)
                sessionModel.MetaData = model;

            this.AddToSession("ProjectInfo", sessionModel); //add metadata to the session
            return RedirectToAction("AuthorList");
        }

        public ActionResult AuthorList()
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            return this.CheckViewFirst((sessionModel.Authors != null) ? sessionModel.Authors : new List<AuthorModel>());
        }

        public ActionResult AddAuthor()
        {
            return this.CheckViewFirst();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAuthor(AuthorModel model)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.Authors == null)
            {
                sessionModel.Authors = new List<AuthorModel>();
            }

            sessionModel.Authors.Add(model); //add the new model to the list

            this.AddToSession("ProjectInfo", sessionModel); //save in session
            return RedirectToAction("AuthorList");
        }

        public ActionResult RemoveAuthor(int id)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.Authors != null)
            {
                sessionModel.Authors.RemoveAt(id);
                this.AddToSession("ProjectInfo", sessionModel); //save in session
            }

            return RedirectToAction("AuthorList");
        }

        public ActionResult EditAuthor(int id)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            if (sessionModel.Authors != null)
            {
                if (sessionModel.Authors[id] != null)
                {
                    AuthorModel model = sessionModel.Authors[id];
                    return View(model);
                }
            }

            return this.CheckViewFirst("AddAuthor");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAuthor(int id, AuthorModel model)
        {
            SessionModelCollector sessionModel = this.GetFromSession<SessionModelCollector>("ProjectInfo");
            sessionModel.Authors[id] = model;
            this.AddToSession("ProjectInfo", sessionModel); //save in session
            return RedirectToAction("AuthorList");
        }
    }
}
