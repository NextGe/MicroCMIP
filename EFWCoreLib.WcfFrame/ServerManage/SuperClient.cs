using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.WcfFrame.WcfHandler;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 超级客户端
    /// </summary>
    public class SuperClient
    {
        //private static string publishServiceName = "CreateDataClientLink";//发布订阅服务名称
        public static ClientLink superClientLink;//连接上级中间件超级客户端
        private static Object syncObj = new Object();//定义一个静态对象用于线程部份代码块的锁定，用于lock操作

        public static void Start()
        {
            CreateSuperClient();
            //初始化连接池,默认10分钟清理连接
            ClientLinkPoolCache.Init(true, 100, 30, 600, "dataclient", 30);

            if (superClientLink != null)
            {
                superClientLink.ReConnectionAction = (()=>
                {
                    SubscriberManager.ReSubscribeAll();//重新订阅
                });
            }
        }

        public static void Stop()
        {
            UnCreateSuperClient();
            UnAllDataClient();
        }

        private static void CreateSuperClient()
        {
            //就算上级中间件重启了，下级中间件创建链接的时候会重新注册本地插件
            superClientLink = new ClientLink(WcfGlobal.HostName, "SuperPlugin", null, WcfGlobal.Identify, null);
            try
            {
                superClientLink.CreateConnection();
            }
            catch (Exception e)
            {
                MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Red, "连接上级中间件失败！" + e.Message);
            }
        }

        private static void UnCreateSuperClient()
        {
            if (superClientLink != null)
                superClientLink.Dispose();
        }

        /// <summary>
        /// 创建数据处理客户端连接，向上级节点请求数据
        /// </summary>
        /// <returns></returns>
        public static ClientLink CreateDataClient()
        {
            //获取的池子索引
            int? index = null;
            ClientLink clientlink = null;
            ClientLinkPool pool = fromPoolGetClientLink("DataPlugin", out clientlink, out index);

            bool IsValid = true;//是否有效
            if (clientlink.ClientObj == null)
            {
                IsValid = false;
            }
            if (clientlink.ClientObj.WcfService.State == CommunicationState.Opening || clientlink.ClientObj.ClientID == null)//解决并发问题
            {
                IsValid = false;
            }

            if (clientlink.ClientObj.WcfService.State == CommunicationState.Closed || clientlink.ClientObj.WcfService.State == CommunicationState.Faulted)
            {
                IsValid = false;
            }

            if (IsValid == false)
            {
                pool.RemovePoolAt("DataPlugin", index);
                Thread.Sleep(400);
                return CreateDataClient();
            }
            clientlink.BeginIdentify = WcfGlobal.Identify;
            return clientlink;
        }
        /// <summary>
        /// 创建数据处理回调连接，向下级节点回调数据
        /// </summary>
        /// <returns></returns>
        public static IDataReply CreateDataReply(string ServerIdentify)
        {
            ClientInfo client;
            lock (syncObj)
            {
                //获取到下级节点的超级连接
                client = ClientManage.ClientDic.Values.ToList().Find(x => x.IsMNode == true && x.ServerIdentify == ServerIdentify);
            }

            if (client != null)
            {
                //获取到新创建的数据连接ClientID
                string clientID = client.dataReply.CreateDataClient();
                //获取到数据连接
                client = ClientManage.ClientDic.Values.ToList().Find(x => x.IsDataClient == true && x.clientId == clientID);
                return client.dataReply;
            }
            return null;
        }

        private static void UnAllDataClient()
        {
            ClientLinkPool pool = ClientLinkPoolCache.GetClientPool("dataclient");
            pool.ClearPool();
        } 

        private static ClientLinkPool fromPoolGetClientLink(string wcfpluginname, out ClientLink clientlink, out int? index)
        {
            ClientLinkPool pool = ClientLinkPoolCache.GetClientPool("dataclient");
            //获取的池子索引
            index = null;
            clientlink = null;
            //是否超时
            bool isouttime = false;
            //超时计时器
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                bool isReap = true;
                //先判断池子中是否有此空闲连接
                if (pool.GetFreePoolNums(wcfpluginname) > 0)
                {
                    isReap = false;
                    clientlink = pool.GetClientLink(wcfpluginname);
                    if (clientlink != null)
                    {
                        index = clientlink.Index;
                    }
                }
                //如果没有空闲连接判断是否池子是否已满，未满，则创建新连接并装入连接池
                //不要一下子将所有连接都在wcf服务创建，逐步创建连接
                if (clientlink == null && !pool.IsPoolFull && pool.GetOpeningNums(wcfpluginname) <= 100)
                {
                    //装入连接池
                    bool flag = pool.AddPool(wcfpluginname,null, out clientlink, out index);
                }

                //如果当前契约无空闲连接，并且队列已满，并且非当前契约有空闲，则踢掉一个非当前契约
                if (clientlink == null && pool.IsPoolFull && pool.GetFreePoolNums(wcfpluginname) == 0 && pool.GetUsedPoolNums(wcfpluginname) != 500)
                {
                    //创建新连接
                    pool.RemovePoolOneNotAt(wcfpluginname, null, out clientlink, out index);
                }

                if (clientlink != null)
                    break;

                //如果还未获取连接判断是否超时30秒，如果超时抛异常
                if (sw.Elapsed >= new TimeSpan(30 * 1000 * 10000))
                {
                    isouttime = true;
                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            sw.Stop();
            sw = null;

            if (isouttime)
            {
                throw new Exception("获取连接池中的连接超时");
            }

            return pool;
        }

    }
}
