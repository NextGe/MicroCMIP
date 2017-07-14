using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.WcfHandler;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 客户端管理
    /// </summary>
    public class ClientManage
    {
        public static bool IsHeartbeat = false;//开启心跳
        public static int HeartbeatTime = 1;//默认间隔1秒,客户端5倍

        //客户端列表
        public static Dictionary<string, ClientInfo> ClientDic = new Dictionary<string, ClientInfo>();
        
        private static Object syncObj = new Object();//定义一个静态对象用于线程部份代码块的锁定，用于lock操作
        /// <summary>
        /// 开始服务主机
        /// </summary>
        public static void StartHost()
        {
            //开启心跳监测
            if (timer == null)
                StartListenClients();
            else
                timer.Start();
        }
        /// <summary>
        /// 停止服务主机
        /// </summary>
        public static void StopHost()
        {
            foreach (ClientInfo client in ClientDic.Values)
            {
                client.IsConnect = false;
            }
        }

        public static string CreateClient(string clientName, DateTime time, IDataReply dataReply, string plugin, string replyidentify)
        {
            string clientId = Guid.NewGuid().ToString();

            try
            {
                AddClient(clientId, clientName, time, dataReply, plugin, replyidentify);
                return clientId;
            }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException(ex.Source + "：创建客户端运行环境失败！");
            }
        }
        
        public static bool UnClient(string clientId)
        {
            RemoveClient(clientId);
            return true;
        }

        public static bool Heartbeat(string clientId)
        {
            bool b = UpdateHeartbeatClient(clientId);
            if (b == true)
            {
                ReConnectionClient(clientId);
                return true;
            }
            else
                return false;
        }

        



        #region 客户端集合操作

        //public static HostWCFMsgHandler hostwcfMsg;
        private static void AddClient(string clientId, string clientName, DateTime time, IDataReply dataReply, string plugin, string replyidentify)
        {
            lock (syncObj)
            {
                ClientInfo info = new ClientInfo();
                info.clientId = clientId;
                info.clientName = clientName;
                info.startTime = time;
                info.dataReply = dataReply;
                info.IsConnect = true;
                info.plugin = plugin;
                info.ServerIdentify = replyidentify;

                ClientDic.Add(clientId, info);
            }
            ShowMsg(Color.Blue, DateTime.Now, "客户端[" + clientName + "]已连接WCF服务主机");
        }
        public static bool UpdateRequestClient(string clientId, int rlen, int slen)
        {
            lock (syncObj)
            {
                if (ClientDic.ContainsKey(clientId))
                {

                    ClientDic[clientId].RequestCount += 1;
                    ClientDic[clientId].receiveData += rlen;
                    ClientDic[clientId].sendData += slen;
                }
            }
            return true;
        }
        private static bool UpdateHeartbeatClient(string clientId)
        {
            lock (syncObj)
            {
                if (ClientDic.ContainsKey(clientId))
                {

                    ClientDic[clientId].startTime = DateTime.Now;
                    ClientDic[clientId].HeartbeatCount += 1;
                    return true;
                }
                else
                    return false;
            }
        }
        private static void RemoveClient(string clientId)
        {
            lock (syncObj)
            {
                if (ClientDic.ContainsKey(clientId))
                {

                    ClientDic.Remove(clientId);
                    ShowMsg(Color.Blue, DateTime.Now, "客户端[" + clientId + "]已退出断开连接WCF服务主机");
                }
            }
        }
        private static void ReConnectionClient(string clientId)
        {
            lock (syncObj)
            {
                if (ClientDic.ContainsKey(clientId))
                {
                    if (ClientDic[clientId].IsConnect == false)
                    {
                        ShowMsg(Color.Blue, DateTime.Now, "客户端[" + clientId + "]已重新连接WCF服务主机");
                        ClientDic[clientId].IsConnect = true;
                    }
                }
            }
        }
        private static void DisConnectionClient(string clientId)
        {
            lock (syncObj)
            {
                if (ClientDic.ContainsKey(clientId))
                {
                    if (ClientDic[clientId].IsConnect == true)
                    {
                        ClientDic[clientId].IsConnect = false;
                        ShowMsg(Color.Blue, DateTime.Now, "客户端[" + clientId + "]已超时断开连接WCF服务主机");
                    }
                }
            }
        }

        #endregion

        #region 心跳显示

        //检测客户端是否在线，超时时间为10s
        static System.Timers.Timer timer;
        private static void StartListenClients()
        {
            timer = new System.Timers.Timer();
            timer.Interval = HeartbeatTime * 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer.Enabled = false;
                lock (syncObj)
                {
                    foreach (ClientInfo client in ClientDic.Values)
                    {
                        if (client.startTime.AddSeconds(HeartbeatTime * 10) < DateTime.Now)//断开10秒就置为断开
                        {
                            DisConnectionClient(client.clientId);
                        }

                        if (client.startTime.AddSeconds(HeartbeatTime * 20) < DateTime.Now)//断开10分钟直接移除客户端
                        {
                            RemoveClient(client.clientId);
                        }
                    }
                }
                timer.Enabled = true;
            }
            catch (Exception err)
            {
                CoreFrame.Common.MiddlewareLogHelper.WriterLog(err.Message+err.StackTrace);
                timer.Enabled = true;
            }
        }
        #endregion

        private static void ShowMsg(Color clr, DateTime time, string text)
        {
            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, clr, text);
        }
    }

    /// <summary>
    /// 连接客户端信息
    /// </summary>
    public class ClientInfo : MarshalByRefObject, ICloneable
    {
        /// <summary>
        /// 客户端标识
        /// </summary>
        public string clientId { get; set; }
        /// <summary>
        /// 客户端名称
        /// </summary>
        public string clientName { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime startTime { get; set; }
        /// <summary>
        /// 心跳次数
        /// </summary>
        public int HeartbeatCount { get; set; }
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnect { get; set; }
        /// <summary>
        /// 请求次数
        /// </summary>
        public int RequestCount { get; set; }
        /// <summary>
        /// 接收数据长度
        /// </summary>
        public long receiveData { get; set; }
        /// <summary>
        /// 发送数据长度
        /// </summary>
        public long sendData { get; set; }
        /// <summary>
        /// 插件名称
        /// </summary>
        public string plugin { get; set; }
        /// <summary>
        /// 中间件标识，只有超级客户端才有值
        /// </summary>
        public string ServerIdentify { get; set; }
        /// <summary>
        /// 是否为中间件节点
        /// </summary>
        public bool IsMNode
        {
            get
            {
                if (plugin == "SuperPlugin" && string.IsNullOrEmpty(ServerIdentify)==false)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// 数据处理客户端
        /// </summary>
        public bool IsDataClient
        {
            get
            {
                if (plugin == "DataPlugin" && string.IsNullOrEmpty(ServerIdentify)==false)
                {
                    return true;
                }
                else
                    return false;
            }
        }


        /// <summary>
        /// 数据回调
        /// </summary>
        public IDataReply dataReply { get; set; }

        #region ICloneable 成员

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }


}
