using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EFWCoreLib.WebFrame.WebAPI
{
    public class TimingActionFilter : ActionFilterAttribute
    {
        private const string Key = "__action_duration__";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (WebApiFrame.WebApiGlobal.IsDebug)
            {
                var stopWatch = new Stopwatch();
                actionContext.Request.Properties[Key] = stopWatch;
                stopWatch.Start();

                WebApiSelfHosting.ShowHostMsg(Color.Black, DateTime.Now, "WebApi开始执行：[" + actionContext.Request.RequestUri.LocalPath + "]");

            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (!actionExecutedContext.Request.Properties.ContainsKey(Key))
            {
                return;
            }

            var stopWatch = actionExecutedContext.Request.Properties[Key] as Stopwatch;
            if (stopWatch != null)
            {
                stopWatch.Stop();
                //var actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
                //var controllerName = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                WebApiSelfHosting.ShowHostMsg(Color.Green, DateTime.Now, "WebApi执行完成(耗时[" + stopWatch.Elapsed.TotalMilliseconds + "])：[" + actionExecutedContext.Request.RequestUri.LocalPath + "]");
                //Debug.Print(string.Format("[Execution of{0}- {1} took {2}.]", controllerName, actionName, stopWatch.Elapsed));
            }
            if (actionExecutedContext.Response != null)
            {
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Methods", "get, put, post, delete, options");
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Headers", "authorization, origin, content-type, accept");
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            }
        }
    }
}
