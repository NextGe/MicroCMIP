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
                try
                {
                    item.callback.Notify(publishServiceName);
                }
                catch
                {
                    //subscriberList.Remove(item);
                }
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
                try
                {
                    sub.callback.Notify(sub.publishServiceName);
                }
                catch
                {
                    //subscriberList.Remove(sub);
                }
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
