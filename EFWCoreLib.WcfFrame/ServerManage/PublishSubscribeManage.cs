using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EFWCoreLib.CoreFrame.Business.AttributeInfo;
using EFWCoreLib.CoreFrame.Common;
//using EFWCoreLib.WcfFrame.ClientController;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.ServerController;
using EFWCoreLib.WcfFrame.Utility.Upgrade;
using EFWCoreLib.WcfFrame.WcfHandler;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 发布者管理
    /// </summary>
    public class PublisherManage
    {
        private static Object syncObj = new Object();//定义一个静态对象用于线程部份代码块的锁定，用于lock操作
        //public static string publishSubscibefile = System.Windows.Forms.Application.StartupPath + "\\Config\\PublishSubscibe.xml";
        private static List<Subscriber> subscriberList;//订阅者列表
        private static List<PublishServiceObject> serviceList;//服务列表
        /// <summary>
        /// 开启订阅服务
        /// </summary>
        public static void Start()
        {
            subscriberList = new List<Subscriber>();//订阅者列表
            serviceList = new List<PublishServiceObject>();//服务列表
        }
        /// <summary>
        /// 停止订阅服务
        /// </summary>
        public static void Stop()
        {

        }

        /// <summary>
        /// 增加服务
        /// </summary>
        public static void AddPublishService(PublishServiceObject serviceObj)
        {
            if (serviceObj == null) return;
            if (serviceList.FindIndex(x => x.publishServiceName == serviceObj.publishServiceName) == -1)
            {
                serviceList.Add(serviceObj);
            }
        }
        /// <summary>
        /// 移除服务
        /// </summary>
        public static void RemovePublishService(string psName)
        {
            if (string.IsNullOrEmpty(psName)) return;
            PublishServiceObject pso = serviceList.Find(x => x.publishServiceName == psName);
            if (pso != null)
            {
                serviceList.Remove(pso);
            }
        }

        /// <summary>
        /// 服务端发布订阅服务列表
        /// </summary>
        /// <returns></returns>
        public static List<PublishServiceObject> GetPublishServiceList()
        {
            return serviceList;
        }


        /// <summary>
        /// 客户端订阅
        /// </summary>
        /// <param name="ServerIdentify"></param>
        /// <param name="clientId"></param>
        /// <param name="publishServiceName"></param>
        /// <param name="callback"></param>
        public static void Subscribe(string ServerIdentify, string publishServiceName, IDataReply callback)
        {
            if (ServerIdentify == WcfGlobal.Identify) return;//不能订阅自己
            UnSubscribe(ServerIdentify, publishServiceName);//先卸载掉
            //然后重新订阅
            Subscriber sub;
            lock (syncObj)
            {
                sub = new Subscriber();
                sub.publishServiceName = publishServiceName;
                sub.callback = callback;
                sub.ServerIdentify = ServerIdentify;
                subscriberList.Add(sub);
            }
            ShowHostMsg(Color.Blue, DateTime.Now, "订阅者[" + ServerIdentify + "]订阅“" + publishServiceName + "”服务成功！");
            SendNotify(sub);
        }
        /// <summary>
        /// 客户端取消订阅
        /// </summary>
        /// <param name="ServerIdentify"></param>
        /// <param name="publishServiceName"></param>
        public static void UnSubscribe(string ServerIdentify, string publishServiceName)
        {
            if (subscriberList.FindIndex(x => x.ServerIdentify == ServerIdentify && x.publishServiceName == publishServiceName) != -1)
            {
                lock (syncObj)
                {
                    Subscriber sub = subscriberList.Find(x => x.ServerIdentify == ServerIdentify && x.publishServiceName == publishServiceName);
                    subscriberList.Remove(sub);
                }
                ShowHostMsg(Color.Black, DateTime.Now, "订阅者[" + ServerIdentify + "]取消订阅“" + publishServiceName + "”服务成功！");
            }
        }
        /// <summary>
        /// 服务端给所有订阅者发送通知
        /// </summary>
        public static void SendNotify(string publishServiceName)
        {
            List<Subscriber> list = subscriberList.FindAll(x => x.publishServiceName == publishServiceName);
            foreach (var item in list)
            {
                item.callback.Notify(publishServiceName);
            }
            ShowHostMsg(Color.Blue, DateTime.Now, "向所有订阅者发送了“" + publishServiceName + "”服务通知！");
        }
        /// <summary>
        /// 服务端给指定订阅者发布通知
        /// </summary>
        /// <param name="sub"></param>
        public static void SendNotify(Subscriber sub)
        {
            if (sub != null)
            {
                sub.callback.Notify(sub.publishServiceName);
                ShowHostMsg(Color.Blue, DateTime.Now, "向订阅者[" + sub.ServerIdentify + "]发送了“" + sub.publishServiceName + "”服务通知！");
            }
        }
        /*
        private static List<PublishServiceObject> LoadXML()
        {
            List<PublishServiceObject> _serviceList = new List<PublishServiceObject>();
            try
            {
                XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(publishSubscibefile);

                XmlNodeList pubservicelist = xmlDoc.DocumentElement.SelectNodes("Publish/service");
                foreach (XmlNode xe in pubservicelist)
                {
                    PublishServiceObject serviceObj = new PublishServiceObject();
                    serviceObj.whether = xe.Attributes["switch"].Value == "1" ? true : false;
                    serviceObj.publishServiceName = xe.Attributes["servicename"].Value;
                    serviceObj.explain = xe.Attributes["explain"].Value;
                    serviceObj.pluginname = xe.Attributes["pluginname"].Value;
                    serviceObj.controller = xe.Attributes["controller"].Value;
                    serviceObj.method = xe.Attributes["method"].Value;
                    serviceObj.argument = xe.Attributes["argument"].Value;
                    _serviceList.Add(serviceObj);
                }
            }
            catch (Exception e)
            {
                MiddlewareLogHelper.WriterLog(LogType.TimingTaskLog, true, System.Drawing.Color.Red, "加载定时任务配置文件错误！");
                MiddlewareLogHelper.WriterLog(LogType.TimingTaskLog, true, System.Drawing.Color.Red, e.Message);
            }

            return _serviceList;
        }
        */
        private static void ShowHostMsg(Color clr, DateTime time, string text)
        {
            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, clr, text);
        }
    }

    /// <summary>
    /// 订阅者处理
    /// </summary>
    public class SubscriberManager
    {
        //public static string publishSubscibefile = System.Windows.Forms.Application.StartupPath + "\\Config\\PublishSubscibe.xml";
        private static List<PublishServiceObject> psoList;//服务端发布的服务
        private static List<SubscribeServiceObject> ssoList;//客户端订阅的服务
        /// <summary>
        /// 开启客户端订阅
        /// </summary>
        public static void Start()
        {
            psoList = new List<PublishServiceObject>();
            ssoList = new List<SubscribeServiceObject>();

           

            //if (psoList != null)
            //{
            //    Dictionary<string, bool> _subDic = LoadXML();
            //    foreach (var item in psoList)
            //    {
            //        SubscribeServiceObject ps = new SubscribeServiceObject();
            //        ps.whether = _subDic.ContainsKey(item.publishServiceName) ? _subDic[item.publishServiceName] : false;
            //        ps.publishServiceName = item.publishServiceName;
            //        ps.explain = item.explain;
            //        ps.IsSub = false;
            //        ssoList.Add(ps);
            //    }
            //}

            ////开始订阅
            //foreach (var i in ssoList)
            //{
            //    if (i.whether)
            //    {
            //        ServerManage.SuperClient.clientLink.Subscribe(i.publishServiceName);
            //        i.IsSub = true;
            //    }
            //}
        }

        /// <summary>
        /// 停止客户端订阅
        /// </summary>
        public static void Stop()
        {

        }

        /// <summary>
        /// 获取发布服务
        /// </summary>
        /// <returns></returns>
        public static List<PublishServiceObject> GetPublishService()
        {
            if (ServerManage.SuperClient.superClientLink != null)
                psoList = ServerManage.SuperClient.superClientLink.GetPublishServiceList();
            return psoList;
        }

        /// <summary>
        /// 获取订阅服务
        /// </summary>
        /// <returns></returns>
        public static List<SubscribeServiceObject> GetSubscribeService()
        {
            return ssoList;
        }

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="ssObject"></param>
        public static void Subscribe(SubscribeServiceObject ssObject)
        {
            if (ssObject == null) return;
            
            if (ssoList.FindIndex(x => x.publishServiceName == ssObject.publishServiceName) == -1)
            {
                ServerManage.SuperClient.superClientLink.Subscribe(ssObject.publishServiceName);
                ssoList.Add(ssObject);
            }
        }

        /// <summary>
        /// 重新订阅所有
        /// </summary>
        /// <param name="ssObject"></param>
        public static void ReSubscribeAll()
        {
            foreach(var sso in ssoList)
            {
                ServerManage.SuperClient.superClientLink.Subscribe(sso.publishServiceName);
            }
        }
        /// <summary>
        /// 取消订阅服务
        /// </summary>
        /// <param name="publishServiceName"></param>
        public static void UnSubscribe(string publishServiceName)
        {
            if (string.IsNullOrEmpty(publishServiceName)) return;
            if (ssoList.FindIndex(x => x.publishServiceName == publishServiceName) == -1) return;
            ServerManage.SuperClient.superClientLink.UnSubscribe(publishServiceName);
            SubscribeServiceObject ssObject = ssoList.Find(x => x.publishServiceName == publishServiceName);
            ssoList.Remove(ssObject);
        }

        /// <summary>
        /// 客户端接收通知
        /// </summary>
        /// <param name="publishServiceName">订阅服务名称</param>
        /// <param name="_clientLink">客户端连接</param>
        public static void ReceiveNotify(string publishServiceName, ClientLink _clientLink)
        {
            ShowHostMsg(Color.Blue, DateTime.Now, "收到订阅的“" + publishServiceName + "”服务通知！");
            if (ssoList.FindIndex(x => x.publishServiceName == publishServiceName) == -1) return;
            //执行订阅服务
            SubscribeServiceObject ssObject = ssoList.Find(x => x.publishServiceName == publishServiceName);
            if (ssObject.ProcessService != null)
            {
                new Action(delegate()
                {
                    //异步执行
                    ssObject.ProcessService(_clientLink);
                }).BeginInvoke(null,null);
            }
        }


        /*
        /// <summary>
        /// 客户端执行订阅服务
        /// </summary>
        /// <param name="_clientLink"></param>
        private static void ProcessPublishService(string publishServiceName, ClientLink _clientLink)
        {
            switch (publishServiceName)
            {
                case "DistributedCache"://分布式缓存服务
                    List<CacheIdentify> ciList = DistributedCacheClient.GetCacheIdentifyList();
                    List<CacheObject> coList = _clientLink.GetDistributedCacheData(ciList);
                    if (coList.Count > 0)
                    {
                        DistributedCacheClient.SetCacheObjectList(coList);
                    }
                    break;
                case "RemotePlugin"://远程插件服务
                    RemotePluginClient.RegisterRemotePlugin();
                    break;
                case "UpgradeClient"://客户端升级
                    ClientUpgradeManager.DownLoadUpgrade();
                    break;
                case "UpgradeServer"://中间件升级
                    break;
                case "MongodbSync"://同步Mongodb数据
                    break;
                case "MiddlewareMonitor"://中间件集群监控服务
                    break;
                case "MiddlewareCmd"://中间件命令服务
                    break;
                default:
                    //PublishServiceObject pso = psoList.Find(x => x.publishServiceName == publishServiceName);
                    //MiddlewareLogHelper.WriterLog(LogType.MidLog, true, System.Drawing.Color.Blue, string.Format("正在执行服务{0}/{1}/{2}/{3}", pso.pluginname, pso.controller, pso.method, pso.argument));
                    //ServiceResponseData retjson = InvokeWcfService(
                    //    pso.pluginname
                    //    , pso.controller
                    //    , pso.method
                    //    , (ClientRequestData request) =>
                    //    {
                    //        request.SetJsonData(pso.argument);
                    //    });
                    //string txtResult = retjson.GetJsonData();
                    //MiddlewareLogHelper.WriterLog(LogType.MidLog, true, System.Drawing.Color.Blue, string.Format("服务执行完成，返回结果：{0}", txtResult));
                    break;
            }

            ShowHostMsg(Color.Blue, DateTime.Now, "执行“" + publishServiceName + "”订阅服务成功！");
        }

        /// <summary>
        /// 手动执行发布服务
        /// </summary>
        /// <param name="psname">服务名称</param>
        public static void ExecPublishService(string psname)
        {
            ProcessPublishService(psname, SuperClient.clientLink);
        }

        
        private static Dictionary<string, bool> LoadXML()
        {
            Dictionary<string, bool> _subDic = new Dictionary<string, bool>();
            try
            {
                XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(publishSubscibefile);

                XmlNodeList subservicelist = xmlDoc.DocumentElement.SelectNodes("Subscibe/service");
                foreach (XmlNode xe in subservicelist)
                {
                    _subDic.Add(xe.Attributes["servicename"].Value, xe.Attributes["switch"].Value == "1" ? true : false);
                }
            }
            catch (Exception e)
            {
                MiddlewareLogHelper.WriterLog(LogType.TimingTaskLog, true, System.Drawing.Color.Red, "加载定时任务配置文件错误！");
                MiddlewareLogHelper.WriterLog(LogType.TimingTaskLog, true, System.Drawing.Color.Red, e.Message);
            }

            return _subDic;
        }

        private static ServiceResponseData InvokeWcfService(string wcfpluginname, string wcfcontroller, string wcfmethod, Action<ClientRequestData> requestAction)
        {
            ClientLink wcfClientLink = ClientLinkManage.CreateConnection(wcfpluginname);
            //绑定LoginRight
            Action<ClientRequestData> _requestAction = ((ClientRequestData request) =>
            {
                request.LoginRight = new EFWCoreLib.CoreFrame.Business.SysLoginRight();
                if (requestAction != null)
                    requestAction(request);
            });
            ServiceResponseData retData = wcfClientLink.Request(wcfcontroller, wcfmethod, _requestAction);
            return retData;
        }
            */

        private static void ShowHostMsg(Color clr, DateTime time, string text)
        {
            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, clr, text);
        }

        
    }

    
    /// <summary>
    /// 订阅者对象
    /// </summary>
    public class Subscriber
    {
        /// <summary>
        /// 订阅者标识
        /// </summary>
        public string ServerIdentify { get; set; }
        /// <summary>
        /// 客户端标识
        /// </summary>
        //public string clientId { get; set; }
        /// <summary>
        /// 发布服务名称
        /// </summary>
        public string publishServiceName { get; set; }
        /// <summary>
        /// 回调通知客户端
        /// </summary>
        public IDataReply callback { get; set; }
    }
    

    /// <summary>
    /// 订阅服务对象
    /// </summary>
    public class SubscribeServiceObject
    {
        
        /// <summary>
        /// 服务名称
        /// </summary>
        public string publishServiceName { get; set; }
        /// <summary>
        /// 服务说明
        /// </summary>
        //public string explain { get; set; }

        /// <summary>
        /// 处理服务
        /// </summary>
        public Action<ClientLink> ProcessService { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        //public bool whether { get; set; }
        /// <summary>
        /// 是否订阅
        /// </summary>
        //public bool IsSub { get; set; }
        /// <summary>
        /// 订阅状态
        /// </summary>
        //public string SubState
        //{
        //    get
        //    {
        //        if (IsSub)
        //            return "已订阅";
        //        else
        //            return "未订阅";
        //    }
        //}
    }

}
