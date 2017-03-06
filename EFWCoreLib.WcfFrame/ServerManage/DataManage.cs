using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Business;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.CoreFrame.Init.AttributeManager;
using EFWCoreLib.CoreFrame.Plugin;
using EFWCoreLib.CoreFrame.SSO;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.SDMessageHeader;
using EFWCoreLib.WcfFrame.WcfHandler;
using Newtonsoft.Json;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 数据处理
    /// </summary>
    public class DataManage
    {
        //处理请求
        public static string ProcessRequest(string clientId, string plugin, string controller, string method, string jsondata, HeaderParameter para)
        {
            string retJson = null;
            try
            {
                //显示调试信息
                if (WcfGlobal.IsDebug == true)
                    ShowHostMsg(Color.Black, DateTime.Now, "服务正在执行：" + controller + "." + method + "(" + jsondata + ")");

                if (para.NodePath == null)//首次请求验证
                {
                    FirstVerification(clientId, plugin, controller, method, jsondata, para);
                    retJson = PathFirstRequest(plugin, controller, method, jsondata, para);
                }
                else//由远程执行服务发送请求
                {
                    retJson = PathNextRequest(plugin, controller, method, jsondata, para);
                }

                //显示调试信息
                if (WcfGlobal.IsDebug == true)
                    ShowHostMsg(Color.Green, DateTime.Now, "服务" + controller + "." + method + "执行完成：" + retJson);

                //更新客户端信息
                ClientManage.UpdateRequestClient(clientId, jsondata == null ? 0 : jsondata.Length, retJson == null ? 0 : retJson.Length);


                return retJson;
            }
            catch (Exception err)
            {
                //记录错误日志
                if (err.InnerException == null)
                {
                    retJson = "{\"flag\":1,\"msg\":" + "\"" + err.Message + "\"" + "}";
                    if (para.iscompressjson)
                    {
                        retJson = ZipComporessor.Compress(retJson);
                    }
                    ShowHostMsg(Color.Red, DateTime.Now, "服务" + controller + "." + method + "执行失败：" + err.Message);
                    return retJson;
                }
                else
                {
                    retJson = "{\"flag\":1,\"msg\":" + "\"" + err.InnerException.Message + "\"" + "}";
                    if (para.iscompressjson)
                    {
                        retJson = ZipComporessor.Compress(retJson);
                    }
                    ShowHostMsg(Color.Red, DateTime.Now, "服务" + controller + "." + method + "执行失败：" + err.InnerException.Message);
                    return retJson;
                }
            }
        }

        //首次验证
        private static void FirstVerification(string clientId, string plugin, string controller, string method, string jsondata, HeaderParameter para)
        {
            if (ClientManage.ClientDic.ContainsKey(clientId) == false)
                throw new Exception("客户端连接已失效！");

            if (plugin == null || controller == null)
                throw new Exception("插件名称或控制器名称不能为空!");

            if (WcfGlobal.IsToken == true)//非调试模式下才验证
            {
                //验证身份，创建连接的时候验证，请求不验证
                IsAuth(plugin, controller, method, para.token);
            }
        }
        //路径首次执行
        private static string PathFirstRequest(string plugin, string controller, string method, string jsondata, HeaderParameter para)
        {
            string retJson = null;
            try
            {
                //验证本地执行还是远程执行服务
                MNodePlugin localPlugin = RemotePluginManage.GetLocalPlugin();
                if (localPlugin.LocalPlugin.ToList().FindIndex(x => x == plugin) != -1)//本地插件
                {
                    //执行本地数据请求
                    retJson = LocalDataRequest(plugin, controller, method, jsondata, para);
                }
                else if (localPlugin.RemotePlugin.FindIndex(x=>x.PluginName==plugin) != -1)//远程插件
                {
                    List<MNodePath> pathlist = new List<MNodePath>();
                    string[] remoteNodeId = localPlugin.RemotePlugin.Find(x=>x.PluginName==plugin).MNodeIdentify.ToArray();
                    MNodeTree mtree = new MNodeTree();
                    mtree.LoadCache();
                    foreach (string Id in remoteNodeId)
                    {
                        pathlist.Add(mtree.CalculateMNodePath(WcfGlobal.Identify, Id));
                    }
                    MNodePath nodePath = null;
                    if (localPlugin.PathStrategy == 0)//随机策略
                    {
                        Random ro = new Random();
                        int index = ro.Next(0, pathlist.Count);
                        nodePath = pathlist[index];
                    }
                    para.NodePath = nodePath;
                    retJson = PathNextRequest(plugin, controller, method, jsondata, para);
                }
                else
                {
                    throw new Exception("本中间件节点中没有配置此插件：" + plugin);
                }
                return retJson;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        //路径执行到下一个节点
        private static string PathNextRequest(string plugin, string controller, string method, string jsondata, HeaderParameter para)
        {
            string retJson = null;
            try
            {
                para.NodePath.NextStep();//节点路径下一步
                if (para.NodePath.IsEndMNode)//到达终节点
                {
                    //执行本地数据请求
                    retJson = LocalDataRequest(plugin, controller, method, jsondata, para);
                }
                else if (para.NodePath.nextMNodeDirection == "<up>")//向上
                {
                    ClientLink clientlink = SuperClient.CreateDataClient();
                    using (var scope = new OperationContextScope(clientlink.ClientObj.WcfService.InnerDuplexChannel as IContextChannel))
                    {
                        HeaderOperater.AddMessageHeader(OperationContext.Current.OutgoingMessageHeaders, para);
                        retJson = clientlink.ClientObj.WcfService.ProcessRequest(clientlink.ClientObj.ClientID, plugin, controller, method, jsondata);
                    }
                }
                else if (para.NodePath.nextMNodeDirection == "<down>")//向下
                {
                    IDataReply dataReply = SuperClient.CreateDataReply(para.NodePath.nextMNode);
                    retJson = dataReply.ReplyProcessRequest(para, plugin, controller, method, jsondata);
                }

                return retJson;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        //本地数据请求
        private static string LocalDataRequest(string plugin, string controller, string method, string jsondata, HeaderParameter para)
        {
            string retJson = null;
            try
            {

                //begintime();
                //超时计时器
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //验证本地执行还是远程执行服务
                MNodePlugin localPlugin = RemotePluginManage.GetLocalPlugin();
                if (localPlugin.LocalPlugin.ToList().FindIndex(x => x == plugin) != -1)//本地插件
                {
                    //执行本地数据请求
                    retJson = ExecuteService(plugin, controller, method, jsondata, para);
                }
                else
                {
                    throw new Exception("本地插件中没找到此插件：" + plugin);
                }

                //double outtime = endtime();
                //记录超时的方法
                if (WcfGlobal.IsOverTime == true)
                {
                    if (sw.Elapsed >= new TimeSpan(WcfGlobal.OverTime * 1000 * 10000))
                    {
                        WriterOverTimeLog(sw.Elapsed.TotalMilliseconds, controller + "." + method + "(" + jsondata + ")");
                    }
                }
                //显示调试信息
                if (WcfGlobal.IsDebug == true)
                    ShowHostMsg(Color.Green, DateTime.Now, "服务" + controller + "." + method + "(耗时[" + sw.Elapsed.TotalMilliseconds + "])");

                //更新客户端信息
                //ClientManage.UpdateRequestClient(clientId, jsondata == null ? 0 : jsondata.Length, retJson == null ? 0 : retJson.Length);


                if (retJson == null)
                    throw new Exception("插件执行未返回有效数据");

                return retJson;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        //执行服务核心方法
        private static string ExecuteService(string plugin, string controller, string method, string jsondata, HeaderParameter para)
        {
            string retJson = null;
            try
            {
                #region 执行插件控制器的核心算法
                object[] paramValue = null;//jsondata?
                ServiceResponseData retObj = null;

                //先解密再解压
                string _jsondata = jsondata;
                //解密参数
                if (para.isencryptionjson)
                {
                    DESEncryptor des = new DESEncryptor();
                    des.InputString = _jsondata;
                    des.DesDecrypt();
                    _jsondata = des.OutString;
                }
                //解压参数
                if (para.iscompressjson)
                {
                    _jsondata = ZipComporessor.Decompress(_jsondata);
                }

                ClientRequestData requestData = new ClientRequestData(para.iscompressjson, para.isencryptionjson, para.serializetype);
                requestData.SetJsonData(_jsondata);
                requestData.LoginRight = para.LoginRight;
                //获取插件服务
                EFWCoreLib.CoreFrame.Plugin.ModulePlugin moduleplugin = CoreFrame.Init.AppPluginManage.PluginDic[plugin];
                retObj = (ServiceResponseData)moduleplugin.WcfServerExecuteMethod(controller, method, paramValue, requestData);

                if (retObj != null)
                {
                    retJson = retObj.GetJsonData();
                }
                else
                {
                    retObj = new ServiceResponseData();
                    retObj.Iscompressjson = para.iscompressjson;
                    retObj.Isencryptionjson = para.isencryptionjson;
                    retObj.Serializetype = para.serializetype;

                    retJson = retObj.GetJsonData();
                }

                retJson = "{\"flag\":0,\"msg\":" + "\"\"" + ",\"data\":" + retJson + "}";
                //先压缩再加密
                //压缩结果
                if (para.iscompressjson)
                {
                    retJson = ZipComporessor.Compress(retJson);
                }
                //加密结果
                if (para.isencryptionjson)
                {
                    DESEncryptor des = new DESEncryptor();
                    des.InputString = retJson;
                    des.DesEncrypt();
                    retJson = des.OutString;
                }


                #endregion

                return retJson;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        //回调请求
        public static string ReplyProcessRequest(string plugin, string controller, string method, string jsondata, HeaderParameter para)
        {
            string retJson = null;
            try
            {
                //显示调试信息
                if (WcfGlobal.IsDebug == true)
                    ShowHostMsg(Color.Black, DateTime.Now, "服务回调：" + controller + "." + method + "(" + jsondata + ")");

                retJson = PathNextRequest(plugin, controller, method, jsondata, para);

                //显示调试信息
                if (WcfGlobal.IsDebug == true)
                    ShowHostMsg(Color.Green, DateTime.Now, "服务" + controller + "." + method + "回调完成：" + retJson);
                return retJson;
            }
            catch (Exception err)
            {
                //记录错误日志
                if (err.InnerException == null)
                {
                    retJson = "{\"flag\":1,\"msg\":" + "\"" + err.Message + "\"" + "}";
                    if (para.iscompressjson)
                    {
                        retJson = ZipComporessor.Compress(retJson);
                    }
                    ShowHostMsg(Color.Red, DateTime.Now, "服务" + controller + "." + method + "回调失败：" + err.Message);
                    return retJson;
                }
                else
                {
                    retJson = "{\"flag\":1,\"msg\":" + "\"" + err.InnerException.Message + "\"" + "}";
                    if (para.iscompressjson)
                    {
                        retJson = ZipComporessor.Compress(retJson);
                    }
                    ShowHostMsg(Color.Red, DateTime.Now, "服务" + controller + "." + method + "回调失败：" + err.InnerException.Message);
                    return retJson;
                }
            }
        }

        private static void ShowHostMsg(Color clr, DateTime time, string text)
        {
            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, clr, text);
        }

        private static void WriterOverTimeLog(double overtime, string text)
        {
            string info = "耗时：" + overtime + "\t\t" + "方法：" + text + "\r\n";
            MiddlewareLogHelper.WriterLog(LogType.OverTimeLog, true, Color.Red, info);
        }

        //每次请求的身份验证，分布式情况下验证麻烦
        private static bool IsAuth(string pname, string cname, string methodname, string token)
        {
            ModulePlugin mp;
            WcfControllerAttributeInfo cattr = AppPluginManage.GetPluginWcfControllerAttributeInfo(pname, cname, out mp);
            if (cattr == null) throw new Exception("插件中没有此控制器名");
            WcfMethodAttributeInfo mattr = cattr.MethodList.Find(x => x.methodName == methodname);
            if (mattr == null) throw new Exception("控制器中没有此方法名");

            if (mattr.IsAuthentication)
            {
                if (token == null)
                    throw new Exception("no token");

                AuthResult result = SsoHelper.ValidateToken(token);
                if (result.ErrorMsg != null)
                    throw new Exception(result.ErrorMsg);

                SysLoginRight loginInfo = new SysLoginRight();
                loginInfo.UserId = Convert.ToInt32(result.User.UserId);
                loginInfo.EmpName = result.User.EmpName;
            }

            return true;
        }

        //向根节点发送数据请求
        public static string RootMNodeProcessRequest(string key, string jsonpara)
        {
            if (WcfGlobal.IsRootMNode)
            {
                switch (key)//扩展更多
                {
                    case "sso_signin":
                    case "sso_signout":
                    case "sso_useractivity":
                        return SsoHelper.ForwardData(key, jsonpara);
                    
                }
                return null;
            }
            else
            {
                return SuperClient.superClientLink.RootRequest(key, jsonpara);
            }
        }
    }

    /// <summary>
    /// 异步结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompletedAsyncResult<T> : IAsyncResult
    {
        T data;

        public CompletedAsyncResult(T data)
        {
            this.data = data;
        }

        public T Data
        { get { return data; } }

        #region IAsyncResult Members
        public object AsyncState
        { get { return (object)data; } }

        public WaitHandle AsyncWaitHandle
        { get { throw new Exception("The method or operation is not implemented."); } }

        public bool CompletedSynchronously
        { get { return true; } }

        public bool IsCompleted
        { get { return true; } }
        #endregion
    }
}
