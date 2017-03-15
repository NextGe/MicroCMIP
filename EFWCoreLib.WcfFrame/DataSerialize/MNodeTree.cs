using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using EFWCoreLib.WcfFrame.ServerManage;
using Newtonsoft.Json;

namespace EFWCoreLib.WcfFrame.DataSerialize
{
    /// <summary>
    /// 中间件节点树
    /// </summary>
    public class MNodeTree
    {
        private string cacheName = "mnodetree";
        /// <summary>
        /// 根中间件节点
        /// </summary>
        public MNodeObject RootMNode
        {
            get
            {
                if (_allMNodeList != null)
                {
                    return _allMNodeList.Find(x => x.ServerIdentify == x.PointToMNode);
                }
                return null;
            }
        }
        private List<MNodeObject> _allMNodeList;
        /// <summary>
        /// 所有中间件节点
        /// </summary>
        public List<MNodeObject> AllMNodeList
        {
            get { return _allMNodeList; }
            set { _allMNodeList = value; }
        }

        /// <summary>
        /// 从Mongodb中加载中间件列表
        /// 从运行中加载中间件节点状态
        /// </summary>
        /// <param name="data">key为中间件标识，value为中间件名称</param>
        public void LoadMongodbAndState(Dictionary<string, string> nodelist_db, List<MNodeObject> nodelist_run)
        {
            //初始化
            _allMNodeList = new List<MNodeObject>();
            if (nodelist_db == null) return;
            //新增节点
            foreach (var n in nodelist_db)
            {
                if (_allMNodeList.FindIndex(x => x.ServerIdentify == n.Key) == -1)
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
                    _allMNodeList.Add(mnodeobj);
                }
            }

            if (nodelist_run == null) return;
            //设置节点的状态
            foreach (var n in _allMNodeList)
            {
                MNodeObject mnodeobj = nodelist_run.Find(x => x.ServerIdentify == n.ServerIdentify);
                if (mnodeobj != null)
                {
                    n.IsConnect = mnodeobj.IsConnect;
                    n.PointToMNode = mnodeobj.PointToMNode;
                }
            }
        }

        /// <summary>
        /// 从缓存中加载中间件树
        /// </summary>
        public void LoadCache()
        {
            //初始化
            _allMNodeList = new List<MNodeObject>();

            CacheObject cobj = DistributedCacheManage.GetLocalCache(cacheName);
            if (cobj != null)
            {
                List<CacheData> cdatalist = cobj.cacheValue;
                foreach (var n in cdatalist)
                {
                    if (n.deleteflag == false && _allMNodeList.FindIndex(x => x.ServerIdentify == n.key) == -1)
                    {
                        MNodeObject mnodeobj = JsonConvert.DeserializeObject<MNodeObject>(n.value);
                        _allMNodeList.Add(mnodeobj);
                    }
                }
            }
        }
        /// <summary>
        /// 同步节点到分布式缓存
        /// </summary>
        public void SyncToCache()
        {
            Dictionary<string, string> sync_adddata = new Dictionary<string, string>();//需要同步的数据
            Dictionary<string, string> sync_updatedata = new Dictionary<string, string>();//需要同步的数据
            List<string> sync_deldata = new List<string>();//需要同步的数据

            CacheObject cobj = DistributedCacheManage.GetLocalCache(cacheName);
            if (cobj != null)
            {
                List<CacheData> cdatalist = cobj.cacheValue;
                //新增的
                foreach (var n in _allMNodeList)
                {
                    if (cdatalist.FindIndex(x => x.key == n.ServerIdentify && x.deleteflag==false) == -1)
                    {
                        sync_adddata.Add(n.ServerIdentify, JsonConvert.SerializeObject(n));
                    }
                }
                //删除的
                foreach (var o in cdatalist)
                {
                    if (o.deleteflag==false && _allMNodeList.FindIndex(x => x.ServerIdentify == o.key) == -1)
                    {
                        sync_deldata.Add(o.key);
                    }
                }

                //更新的
                foreach (var o in cdatalist)
                {
                    MNodeObject o1 = JsonConvert.DeserializeObject<MNodeObject>(o.value);
                    MNodeObject o2 = _allMNodeList.Find(x => x.ServerIdentify == o.key);
                    if(o2!=null && o1.IsConnect!=o2.IsConnect)
                    {
                        sync_updatedata.Add(o.key, JsonConvert.SerializeObject(o2));
                    }
                }

            }
            else
            {
                //新增的
                foreach (var n in _allMNodeList)
                {
                    sync_adddata.Add(n.ServerIdentify, JsonConvert.SerializeObject(n));
                }
            }

            DistributedCacheManage.SetCache(cacheName, sync_adddata,sync_updatedata, sync_deldata);
        }

        /// <summary>
        /// 计算两个节点之间路径
        /// </summary>
        /// <param name="beginNode">开始节点，一般是当前中间件</param>
        /// <param name="endNode">结束节点</param>
        /// <returns></returns>
        public MNodePath CalculateMNodePath(string beginNode,string endNode)
        {
            List<MNodeObject> beginToRoot = new List<MNodeObject>();//开始节点到根节点经过的所有节点
            List<MNodeObject> endToRoot = new List<MNodeObject>();//结束节点到根节点经过的所有节点
            nodeToRoot(beginNode, beginToRoot);
            nodeToRoot(endNode, endToRoot);

            MNodeObject coincideNode=null;//重合点
            foreach (var bn in beginToRoot)
            {
                if (endToRoot.FindIndex(x => x.ServerIdentify == bn.ServerIdentify) == -1)
                    continue;

                if(endToRoot.FindIndex(x => x.ServerIdentify == bn.ServerIdentify) != -1)
                {
                    coincideNode = bn;
                    break;
                }
            }
            if (coincideNode == null) return null;

            List<MNodeObject> pathNodeList = new List<MNodeObject>();//路径节点列表
            //1.添加向上节点
            for(int i = 0; i < beginToRoot.Count; i++)
            {
                if (beginToRoot[i].ServerIdentify == coincideNode.ServerIdentify)
                {
                    break;
                }
                pathNodeList.Add(beginToRoot[i]);
            }
            //2.添加交叉节点
            pathNodeList.Add(coincideNode);
            //3.添加向下节点
            bool flag = false;
            for(int i = endToRoot.Count - 1; i >= 0; i--)
            {
                if (endToRoot[i].ServerIdentify == coincideNode.ServerIdentify && flag == false)
                {
                    flag = true;
                    continue;
                }
                else
                {
                    if (flag == true)
                    {
                        pathNodeList.Add(endToRoot[i]);
                    }
                    continue;
                }
            }

            MNodePath path = new MNodePath(pathNodeList,coincideNode);//转换为节点路径
            return path;
        }
        /// <summary>
        /// 递归寻找路径
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodelist"></param>
        private void nodeToRoot(string node, List<MNodeObject> nodelist)
        {
            MNodeObject mnobj =_allMNodeList.Find(x => x.ServerIdentify == node);
            if (mnobj == null) return;
            if (node == RootMNode.ServerIdentify)
            {
                nodelist.Add(RootMNode);
                return;
            }
            nodelist.Add(mnobj);
            nodeToRoot(mnobj.PointToMNode, nodelist);
        }
    }
    /// <summary>
    /// 服务执行中间件节点路径
    /// </summary>
    [DataContract]
    public class MNodePath
    {
        /// <summary>
        /// efwplus://a/A/一/D
        /// </summary>
        [DataMember]
        public string efwplusPath { get; set; }
        /// <summary>
        /// 路径描叙
        /// efwplus://节点a/节点A/节点一/节点D
        /// </summary>
        [DataMember]
        public string efwplusPathDescribe { get; set; }
        /// <summary>
        /// 路径方向
        /// efwplus://<none>/<up>/<up>/<down>
        /// </summary>
        [DataMember]
        public string efwplusPathDirection { get; set; }
        /// <summary>
        /// 当前节点标识
        /// </summary>
        [DataMember]
        public string currentMNode { get; set; }
        /// <summary>
        /// 下一个节点标识
        /// </summary>
        [DataMember]
        public string nextMNode { get; set; }

        /// <summary>
        /// 下一个节点方向 <none>空 <up>向上 <down>向下 <route>路由
        /// </summary>
        [DataMember]
        public string nextMNodeDirection { get; set; }

        /// <summary>
        /// 是否开始节点
        /// </summary>
        [DataMember]
        public bool IsBeginMNode { get; set; }
        /// <summary>
        /// 是否终节点
        /// </summary>
        [DataMember]
        public bool IsEndMNode { get; set; }

        /// <summary>
        /// 路径下一步
        /// </summary>
        public void NextStep()
        {
            if (string.IsNullOrEmpty(efwplusPath)) return;
            if (efwplusPath.IndexOf("efwplus://") == 0)//正确的路径
            {
                string[] nodes = efwplusPath.Substring("efwplus://".Length).Split('/');//节点
                string[] directions=efwplusPathDirection.Substring("efwplus://".Length).Split('/');//方向
                int index = -1;
                if (string.IsNullOrEmpty(currentMNode))//设置开始节点
                {
                    index = 0;
                }
                else
                {
                    index = nodes.ToList().FindIndex(x => x == currentMNode) + 1;
                }

                if (index == 0)
                    IsBeginMNode = true;
                else
                    IsBeginMNode = false;

                if (index == nodes.Length - 1)
                    IsEndMNode = true;
                else
                    IsEndMNode = false;

                currentMNode = nodes[index];
                if (IsEndMNode)
                {
                    nextMNode = "";
                    nextMNodeDirection = "0";
                }
                else
                {
                    nextMNode = nodes[index+1];
                    nextMNodeDirection = directions[index + 1];
                }
            }
        }

        public MNodePath(List<MNodeObject> pathNodeList, MNodeObject coincideNode)
        {
            efwplusPath = "efwplus://";
            efwplusPathDescribe = "efwplus://";
            efwplusPathDirection = "efwplus://";
            if (pathNodeList == null || pathNodeList.Count == 0 || coincideNode == null) return;
            bool flag = false;
            for (int i = 0; i < pathNodeList.Count; i++)
            {
                efwplusPath += pathNodeList[i].ServerIdentify + "/";
                efwplusPathDescribe += pathNodeList[i].ServerName + "/";


                if (i == 0)
                {
                    efwplusPathDirection += "<none>/";
                }
                else if (flag == true)
                {
                    efwplusPathDirection += "<down>/";
                }
                else
                {
                    efwplusPathDirection += "<up>/";
                }

                if (pathNodeList[i].ServerIdentify == coincideNode.ServerIdentify)
                {
                    flag = true;
                }
            }
            //移除掉最后一个/
            efwplusPath = efwplusPath.Remove(efwplusPath.Length - 1, 1);
            efwplusPathDescribe = efwplusPathDescribe.Remove(efwplusPathDescribe.Length - 1, 1);
            efwplusPathDirection = efwplusPathDirection.Remove(efwplusPathDirection.Length - 1, 1);
        }
    }
}
