using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Mongodb;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.Utility.MonitorPlatform;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 节点状态管理
    /// 必须先启动SuperCient
    /// 1.监控所有子节点状态
    /// 2.将子节点状态通知到所有子节点
    /// </summary>
    public class MNodeStateManage
    {
        //中间件节点状态
        public static List<MNodeObject> MNodeList = new List<MNodeObject>();
        private static Object syncObj = new Object();//定义一个静态对象用于线程部份代码块的锁定，用于lock操作
        /// <summary>
        /// 1.监控所有子节点状态
        /// </summary>
        /// <param name="mnodeList"></param>
        public static void MNodeState(List<MNodeObject> mnodeList)
        {
            lock (syncObj)
            {
                foreach (MNodeObject node in mnodeList)
                {
                    if (MNodeList.FindIndex(x => x.ServerIdentify == node.ServerIdentify) == -1)
                    {
                        MNodeList.Add(node);
                    }
                    else
                    {
                        MNodeObject mnode = MNodeList.Find(x => x.ServerIdentify == node.ServerIdentify);
                        mnode.IsConnect = node.IsConnect;
                        mnode.PointToMNode = WcfGlobal.Identify;
                    }
                }
            }
        }

        /// <summary>
        /// 从缓存中获取中间件树
        /// </summary>
        /// <returns></returns>
        public static MNodeTree GetMNodeTree()
        {
            MNodeTree mtree = new MNodeTree();
            mtree.LoadCache();
            return mtree;
        }

    }

    /// <summary>
    /// 节点状态处理客户端
    /// </summary>
    public class MNodeStateClient
    {
        private static Object syncObj = new Object();//定义一个静态对象用于线程部份代码块的锁定，用于lock操作
        /// <summary>
        /// 获取下级中间件节点状态
        /// </summary>
        public static void GetSubMNodeState()
        {
            lock (syncObj)
            {
                foreach (ClientInfo client in ClientManage.ClientDic.Values)
                {
                    //收集下级中间件节点状态
                    if (client.IsMNode)
                    {
                        if (MNodeStateManage.MNodeList.FindIndex(x => x.ServerIdentify == client.ServerIdentify) == -1)
                        {
                            MNodeObject mnode = new MNodeObject();
                            mnode.ServerIdentify = client.ServerIdentify;
                            mnode.IsConnect = client.IsConnect;
                            mnode.PointToMNode = WcfGlobal.Identify;
                            MNodeStateManage.MNodeList.Add(mnode);
                        }
                        else
                        {
                            MNodeObject mnode = MNodeStateManage.MNodeList.Find(x => x.ServerIdentify == client.ServerIdentify);
                            mnode.IsConnect = client.IsConnect;
                            mnode.PointToMNode = WcfGlobal.Identify;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 发送中间件节点状态到上级节点，包括实时下级+下级发送上来的下下级
        /// </summary>
        public static void SendMNodeStateToSup()
        {
            if (SuperClient.superClientLink != null && WcfGlobal.IsRootMNode == false)//非根节点中间件将节点状态发送到父节点
                SuperClient.superClientLink.MNodeState(MNodeStateManage.MNodeList);
        }

    }
}
