using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AugPServer.Helpers
{
    public static class SessionHelper
    {
        public static void AddToSession(this Controller controller, string key, object value)
        {
            controller.HttpContext.Session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetFromSession<T>(this Controller controller, string key)
        {
            string value = controller.HttpContext.Session.GetString(key);
            return (value == null) ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static void RemoveFromSession(this Controller controller, string key)
        {
            controller.HttpContext.Session.Remove(key);
        }

        public static string SessionId(this Controller controller)
        {
            return controller.HttpContext.Session.Id;
        }
    }
}
