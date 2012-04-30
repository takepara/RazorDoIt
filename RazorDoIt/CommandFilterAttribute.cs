using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace RazorDoIt
{
    public class CommandNameAttribute : ActionNameSelectorAttribute
    {
        public CommandNameAttribute(string name, string command)
        {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }

            Name = name;
            Command = command;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Command
        {
            get;
            private set;
        }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo) 
        {
            return String.Equals(actionName, Name, StringComparison.OrdinalIgnoreCase) && controllerContext.HttpContext.Request.Form[actionName] == Command;
        }    
    }

    public class CommandFilterAttribute : ActionNameSelectorAttribute
    {
        private string _command;

        public CommandFilterAttribute(string command)
        {
            _command = command;
        }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            return controllerContext.HttpContext.Request.Form[actionName] == _command;
        }
    }
}
