using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.ServerManage;
using Newtonsoft.Json;

namespace EFWCoreLib.WcfFrame.ServerManage
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
    
}
