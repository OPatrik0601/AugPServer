using AugPServer.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AugPServer.Helpers
{
    public static class ProjectChecker
    {
        /// <summary>
        /// Checks if the project has been already finished
        /// </summary>
        /// <param name="controller">The controller of the view</param>
        /// <returns>Is it finished?</returns>
        public static bool IsProjectFinished(this Controller controller) {
            SessionModelCollector sessionModel = SessionHelper.GetFromSession<SessionModelCollector>(controller, "ProjectInfo");
            if(sessionModel != null)
            {
                return sessionModel.IsFinished;
            }

            return false;
        }
        
        /// <summary>
        /// If the project has been finished, redirect to the end page.
        /// </summary>
        /// <param name="controller">The controller of the view</param>
        /// <param name="model">The model for the view</param>
        /// <returns>Returns the right view</returns>
        public static ActionResult CheckViewFirst(this Controller controller, object model)
        {
            if(IsProjectFinished(controller))
                return controller.RedirectToAction("NewImages", "FinishNewProject");

            return controller.View(model);
        }

        /// <summary>
        /// If the project has been finished, redirect to the end page.
        /// </summary>
        /// <param name="controller">The controller of the view</param>
        /// <returns>Returns the right view</returns>
        public static ActionResult CheckViewFirst(this Controller controller)
        {
            if (IsProjectFinished(controller))
                return controller.RedirectToAction("NewImages", "FinishNewProject");

            return controller.View();
        }
    }
}
