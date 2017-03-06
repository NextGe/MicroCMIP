using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Init;

namespace EFWCoreLib.WebFrame.WebAPI
{
    public class WebApiSelfHosting
    {
        //public static HostWCFMsgHandler hostwcfMsg;
        private string WebApiUri;
        private HttpSelfHostServer server;

        public WebApiSelfHosting(string _WebApiUri)
        {
            //MiddlewareLogHelper.WriterLog(LogType.WebApiLog,true,Color.Blue, "WebAPI服务正在初始化...");
            //初始化操作
            if (_WebApiUri != null)
                WebApiUri = _WebApiUri;
            else
                WebApiUri = "http://localhost:8088";

            //初始化连接池,默认10分钟清理连接
            WcfFrame.ClientLinkPoolCache.Init(true, 500, 30, 600, "webapi", 30);

            //MiddlewareLogHelper.WriterLog(LogType.WebApiLog, true, Color.Blue, "WebAPI服务初始化完成");
        }

        ~WebApiSelfHosting()
        {
            StopHost();
        }

        public void StartHost()
        {     
            var config = new HttpSelfHostConfiguration(WebApiUri);

            //格式化日期
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(
                   new Newtonsoft.Json.Converters.IsoDateTimeConverter()
                   {
                       DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
                   }
               );
            //config.VirtualPathRoot= AppGlobal.AppRootPath + @"Http\";
           // (config as HttpConfiguration).MapHttpAttributeRoutes();
            config.MaxBufferSize = 2097152;//最大缓存值2M
            config.MaxReceivedMessageSize = 2097152;
            //config.TransferMode = System.ServiceModel.TransferMode.Buffered;

            //
            config.Routes.MapHttpRoute(
                "efwplusApi",
                "EfwApi/{plugin}/{controller}/{action}/{id}",
                new { id = RouteParameter.Optional });

            //小型Web服务器
            config.Routes.MapHttpRoute(
                "MiniHttp",
                "MiniHttp/{id}",
                new { plugin= "coresys", controller = "Http", action = "html",id= RouteParameter.Optional });

            //升级包提供下载
            config.Routes.MapHttpRoute(
               "Upgrade",
               "Upgrade/{id}",
               new { plugin = "coresys", controller = "ClientUpgrade", action = "Upgrade", id = RouteParameter.Optional });

            //登陆接口
            config.Routes.MapHttpRoute(
               "Login",
               "Login/{action}/{id}",
               new { plugin = "coresys", controller = "Login", id = RouteParameter.Optional });

            //监控平台接口
            config.Routes.MapHttpRoute(
               "Monitor",
               "Monitor/{action}/{id}",
               new { plugin = "coresys", controller = "Monitor", id = RouteParameter.Optional });


            //主机配置接口
            config.Routes.MapHttpRoute(
               "MNodeConfig",
               "MNodeConfig/{action}/{id}",
               new { plugin = "coresys", controller = "MNodeConfig", id = RouteParameter.Optional });


            //指定插件的程序集
            config.Services.Replace(typeof(IAssembliesResolver),
                                                   new PluginAssembliesResolver());
            //指定插件名称
            config.Services.Replace(typeof(IHttpControllerSelector),
                                                   new PluginHttpControllerSelector(config));

            //显示异常信息
            config.Filters.Add(new ShowExceptionFilter());
            //调试信息
            config.Filters.Add(new TimingActionFilter());
            //用户令牌验证
            config.Filters.Add(new UserTokenActionFilter());

            server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
        }

        public void StopHost()
        {
            if (server != null)
                server.CloseAsync().Wait();
        }

        public static void ShowHostMsg(Color clr, DateTime time, string text)
        {
            MiddlewareLogHelper.WriterLog(LogType.WebApiLog, true, clr, text);
        }
    }
}
