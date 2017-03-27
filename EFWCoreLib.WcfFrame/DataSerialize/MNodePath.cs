using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EFWCoreLib.WcfFrame.DataSerialize
{
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
                string[] directions = efwplusPathDirection.Substring("efwplus://".Length).Split('/');//方向
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
                    nextMNode = nodes[index + 1];
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
