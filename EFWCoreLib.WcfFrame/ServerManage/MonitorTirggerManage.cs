using EFWCoreLib.CoreFrame.Mongodb;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.Utility.MonitorPlatform;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 监视触发器管理
    /// </summary>
    public class MonitorTirggerManage
    {
        private static System.Timers.Timer timer;
        /// <summary>
        /// 开始
        /// </summary>
        public static void Start()
        {
            if (timer == null)
                StartListen();
            else
                timer.Start();
        }

        
        private static void StartListen()
        {
            timer = new System.Timers.Timer();
            timer.Interval = 1000 * 10;//5s
            //timer.Elapsed -= new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer.Enabled = false;
                MonitorMongoData();
                MonitorMNodeState();
                MonitorMNodeTree();
                MonitorRemotePlugin();
                MonitorUpgrade();
                timer.Enabled = true;
            }
            catch (Exception err)
            {
                CoreFrame.Common.MiddlewareLogHelper.WriterLog(err.Message + err.StackTrace);
                timer.Enabled = true;
            }
        }

        static List<MidNode> mnList;
        static List<MNodePService> npList;
        static List<PluginService> psList;

        /// <summary>
        /// 监视Mongo数据
        /// </summary>
        static void MonitorMongoData()
        {
            if (WcfGlobal.IsRootMNode)
            {
                //从Mongodb获取中间件节点
                MongoHelper<MidNode> helper = new MongoHelper<MidNode>(WcfGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                mnList = helper.FindAll(null);
                MongoHelper<MNodePService> mphelper = new MongoHelper<MNodePService>(WcfGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                npList = mphelper.FindAll(null);
                MongoHelper<PluginService> pshelper = new MongoHelper<PluginService>(WcfGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                psList = pshelper.FindAll(null);
            }
        }

        /// <summary>
        /// 监视中间件节点树
        /// </summary>
        static void MonitorMNodeTree()
        {
            if (WcfGlobal.IsRootMNode)
            {
                #region 更新MNodeStateManage.MNodeList
                if (MNodeStateManage.MNodeList == null) return;
                if (mnList == null) return;

                List<MNodeObject> nodelist_run = MNodeStateManage.MNodeList;

                Dictionary<string, string> MNdic = new Dictionary<string, string>();
                foreach (var n in mnList)
                {
                    //未停用
                    if (string.IsNullOrEmpty(n.identify) == false && n.delflag == 0)
                        MNdic.Add(n.identify, n.nodename);
                }

                //初始化
                List<MNodeObject> nodelist_db = new List<MNodeObject>();
                //新增节点
                foreach (var n in MNdic)
                {
                    if (nodelist_db.FindIndex(x => x.ServerIdentify == n.Key) == -1)
                    {
                        MNodeObject mnodeobj = new MNodeObject();
                        mnodeobj.ServerIdentify = n.Key;
                        mnodeobj.ServerName = n.Value;
                        mnodeobj.IsConnect = false;//默认未开启
                        if (WcfGlobal.IsRootMNode == true && WcfGlobal.Identify == n.Key)//根节点
                        {
                            mnodeobj.PointToMNode = n.Key;
                            mnodeobj.IsConnect = true;//根节点默认开启
                        }
                        else
                            mnodeobj.PointToMNode = null;
                        nodelist_db.Add(mnodeobj);
                    }
                }



                //设置nodelist_db节点的状态
                foreach (var n in nodelist_db)
                {
                    MNodeObject mnodeobj = nodelist_run.Find(x => x.ServerIdentify == n.ServerIdentify);
                    if (mnodeobj != null)
                    {
                        n.IsConnect = mnodeobj.IsConnect;
                        n.PointToMNode = mnodeobj.PointToMNode;
                    }
                }

                #endregion

                string cacheName = "mnodetree";

                #region MNodeStateManage.MNodeList同步到分布式缓存

                Dictionary<string, string> sync_adddata = new Dictionary<string, string>();//需要同步的数据
                Dictionary<string, string> sync_updatedata = new Dictionary<string, string>();//需要同步的数据
                List<string> sync_deldata = new List<string>();//需要同步的数据

                CacheObject cobj = DistributedCacheManage.GetLocalCache(cacheName);
                if (cobj != null)
                {
                    List<CacheData> cdatalist = cobj.cacheValue;
                    //新增的
                    foreach (var n in nodelist_db)
                    {
                        if (cdatalist.FindIndex(x => x.key == n.ServerIdentify && x.deleteflag == false) == -1)
                        {
                            sync_adddata.Add(n.ServerIdentify, JsonConvert.SerializeObject(n));
                        }
                    }
                    //删除的
                    foreach (var o in cdatalist)
                    {
                        if (o.deleteflag == false && nodelist_db.FindIndex(x => x.ServerIdentify == o.key) == -1)
                        {
                            sync_deldata.Add(o.key);
                        }
                    }

                    //更新的
                    foreach (var o in cdatalist)
                    {
                        MNodeObject o1 = JsonConvert.DeserializeObject<MNodeObject>(o.value);
                        MNodeObject o2 = nodelist_db.Find(x => x.ServerIdentify == o.key);
                        if (o2 != null && o1.IsConnect != o2.IsConnect)
                        {
                            sync_updatedata.Add(o.key, JsonConvert.SerializeObject(o2));
                        }
                    }

                }
                else
                {
                    //新增的
                    foreach (var n in nodelist_db)
                    {
                        sync_adddata.Add(n.ServerIdentify, JsonConvert.SerializeObject(n));
                    }
                }

                DistributedCacheManage.SetCache(cacheName, sync_adddata, sync_updatedata, sync_deldata);

                #endregion
            }
        }

        /// <summary>
        /// 监视中间件节点状态
        /// </summary>
        static void MonitorMNodeState()
        {
            MNodeStateClient.GetSubMNodeState();//第一步，获取下级中间件节点状态
            MNodeStateClient.SendMNodeStateToSup();//第二步，发送中间件节点状态到上级节点
        }
        /// <summary>
        /// 监视中间件节点配置的远程插件
        /// </summary>
        static void MonitorRemotePlugin()
        {
            if (WcfGlobal.IsRootMNode)
            {
                #region 加载mpList
                //从Mongodb加载节点服务配置
                List<MNodePlugin> mpList = new List<MNodePlugin>();
                foreach (var ps in npList)
                {
                    MNodePlugin mp = new MNodePlugin();
                    mp.ServerIdentify = ps.identify;
                    mp.PathStrategy = ps.pathstrategy;
                    mp.LocalPlugin = ps.localplugin;
                    mp.RemotePlugin = new List<RemotePlugin>();
                    foreach (var r in ps.remoteplugin)
                    {
                        RemotePlugin rp = new RemotePlugin();
                        rp.PluginName = r.pluginname;
                        rp.MNodeIdentify = r.mnodeidentify;
                        mp.RemotePlugin.Add(rp);
                    }
                    mp.LocalPinfoList = new List<MNPluginInfo>();
                    foreach (string p in mp.LocalPlugin)
                    {
                        PluginService pservice = psList.Find(x => x.pluginname == p);
                        if (pservice != null)
                        {
                            MNPluginInfo pinfo = new MNPluginInfo();
                            pinfo.pluginname = pservice.pluginname;
                            pinfo.title = pservice.title;
                            pinfo.versions = pservice.versions;
                            pinfo.author = pservice.author;
                            pinfo.introduce = pservice.introduce;
                            pinfo.updatenotes = pservice.updatenotes;
                            pinfo.delflag = pservice.delflag;
                            mp.LocalPinfoList.Add(pinfo);
                        }
                    }

                    mpList.Add(mp);
                }
                #endregion
                //CoreFrame.Common.MiddlewareLogHelper.WriterLog(JsonConvert.SerializeObject(mpList));
                #region 同步mpList到分布式缓存
                Dictionary<string, string> sync_adddata = new Dictionary<string, string>();//需要同步的数据
                Dictionary<string, string> sync_updatedata = new Dictionary<string, string>();//需要同步的数据
                List<string> sync_deldata = new List<string>();//需要同步的数据
                string cacheName = "mnodeplugin";

                CacheObject cobj = DistributedCacheManage.GetLocalCache(cacheName);
                if (cobj != null)
                {
                    List<CacheData> cdatalist = cobj.cacheValue;
                    //新增的
                    foreach (var n in mpList)
                    {
                        if (cdatalist.FindIndex(x => x.key == n.ServerIdentify) == -1)
                        {
                            sync_adddata.Add(n.ServerIdentify, JsonConvert.SerializeObject(n));
                        }
                    }
                    //删除的
                    foreach (var o in cdatalist)
                    {
                        if (mpList.FindIndex(x => x.ServerIdentify == o.key) == -1)
                        {
                            sync_deldata.Add(o.key);
                        }
                    }

                    //更新的
                    foreach (var o in cdatalist)
                    {
                        MNodePlugin mp = mpList.Find(x => x.ServerIdentify == o.key);
                        if (mp != null && JsonConvert.SerializeObject(mp) != o.value)
                        {
                            sync_updatedata.Add(o.key, JsonConvert.SerializeObject(mp));
                        }
                    }

                }
                else
                {
                    //新增的
                    foreach (var n in mpList)
                    {
                        sync_adddata.Add(n.ServerIdentify, JsonConvert.SerializeObject(n));
                    }
                }
                //调试
                //foreach(var i in sync_updatedata)
                //{
                //    CoreFrame.Common.MiddlewareLogHelper.WriterLog(i.Key);
                //    CoreFrame.Common.MiddlewareLogHelper.WriterLog(i.Value);
                //}
                DistributedCacheManage.SetCache(cacheName, sync_adddata, sync_updatedata, sync_deldata);
                #endregion
            }
        }

        /// <summary>
        /// 监视升级程序，包括插件升级、中间件升级、Web升级、客户端升级
        /// </summary>
        static void MonitorUpgrade()
        {
            if (WcfGlobal.IsRootMNode)
            {
                UpgradeManage.UpdateUpgrade();
            }
        }
    }
}
