using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json;

namespace EFWCoreLib.CoreFrame.Init
{
    /// <summary>
    /// 主机常用配置
    /// </summary>
    public class HostSettingConfig
    {
        private static System.Xml.XmlDocument xmlDoc = null;
        private static string configfile = System.Windows.Forms.Application.StartupPath + "\\Config\\SettingConfig.xml";

        private static void InitConfig()
        {
            xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(configfile);
        }

        public static string GetValue(string keyname)
        {
            if (xmlDoc == null) InitConfig();
            return xmlDoc.DocumentElement.SelectNodes(keyname)[0].Attributes["value"].Value.ToString();
        }

        public static string GetValue(string keyname, string attrname)
        {
            if (xmlDoc == null) InitConfig();
            return xmlDoc.DocumentElement.SelectNodes(keyname)[0].Attributes[attrname].Value.ToString();
        }

        public static void SetValue(string keyname, string value)
        {
            if (xmlDoc == null) InitConfig();
            xmlDoc.DocumentElement.SelectNodes(keyname)[0].Attributes["value"].Value = value;
        }

        public static void SetValue(string keyname, string attrname, string value)
        {
            if (xmlDoc == null) InitConfig();
            xmlDoc.DocumentElement.SelectNodes(keyname)[0].Attributes[attrname].Value = value;
        }

        public static void SaveConfig()
        {
            if (xmlDoc == null) InitConfig();
            xmlDoc.Save(configfile);
            InitConfig();
        }
    }
    /// <summary>
    /// 服务地址配置
    /// </summary>
    public class HostAddressConfig
    {
        private static string appconfig = AppGlobal.AppRootPath + "efwplusBase.exe.config";
        private static XmlDocument xmldoc_app;

        private static string routeconfig = AppGlobal.AppRootPath + "efwplusRoute.exe.config";
        private static XmlDocument xmldoc_route;

        //private static string webapiconfig = AppGlobal.AppRootPath + "efwplusWebAPI.exe.config";
        //private static XmlDocument xmldoc_webapi;

        private static void InitConfig()
        {
            //appconfig = AppGlobal.AppRootPath + System.IO.Path.GetFileName(Application.ExecutablePath) + ".config";
            xmldoc_app = new XmlDocument();
            xmldoc_app.Load(appconfig);

            xmldoc_route = new XmlDocument();
            xmldoc_route.Load(routeconfig);

            //xmldoc_webapi = new XmlDocument();
            //xmldoc_webapi.Load(webapiconfig);
        }

        public static string GetWcfAddress()
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node;
            node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/services/service[@name='EFWCoreLib.WcfFrame.WcfHandler.BaseService']/host/baseAddresses/add");
            if (node != null)
            {
                return node.Attributes["baseAddress"].Value;
            }
            return null;
        }

        public static void SetWcfAddress(string url)
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/services/service[@name='EFWCoreLib.WcfFrame.WcfHandler.BaseService']/host/baseAddresses/add");
            if (node != null)
            {
                node.Attributes["baseAddress"].Value = url;
            }
        }

        public static string GetFileAddress()
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/services/service[@name='EFWCoreLib.WcfFrame.WcfHandler.FileService']/host/baseAddresses/add");
            if (node != null)
            {
                return node.Attributes["baseAddress"].Value;
            }
            return null;
        }

        public static void SetFileAddress(string url)
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/services/service[@name='EFWCoreLib.WcfFrame.WcfHandler.FileService']/host/baseAddresses/add");
            if (node != null)
            {
                node.Attributes["baseAddress"].Value = url;
            }
        }

        public static string GetRouterAddress()
        {
            if (xmldoc_route == null) InitConfig();
            XmlNode node = xmldoc_route.DocumentElement.SelectSingleNode("system.serviceModel/services/service[@name='EFWCoreLib.WcfFrame.WcfHandler.RouterBaseService']/host/baseAddresses/add");
            if (node != null)
            {
                return node.Attributes["baseAddress"].Value;
            }
            return null;
        }

        public static void SetRouterAddress(string url)
        {
            if (xmldoc_route == null) InitConfig();
            XmlNode node = xmldoc_route.DocumentElement.SelectSingleNode("system.serviceModel/services/service[@name='EFWCoreLib.WcfFrame.WcfHandler.RouterBaseService']/host/baseAddresses/add");
            if (node != null)
            {
                node.Attributes["baseAddress"].Value = url;
            }
        }

        public static string GetfileRouterAddress()
        {
            if (xmldoc_route == null) InitConfig();
            XmlNode node = xmldoc_route.DocumentElement.SelectSingleNode("system.serviceModel/services/service[@name='EFWCoreLib.WcfFrame.WcfHandler.RouterFileService']/host/baseAddresses/add");
            if (node != null)
            {
                return node.Attributes["baseAddress"].Value;
            }
            return null;
        }

        public static void SetfileRouterAddress(string url)
        {
            if (xmldoc_route == null) InitConfig();
            XmlNode node = xmldoc_route.DocumentElement.SelectSingleNode("system.serviceModel/services/service[@name='EFWCoreLib.WcfFrame.WcfHandler.RouterFileService']/host/baseAddresses/add");
            if (node != null)
            {
                node.Attributes["baseAddress"].Value = url;
            }
        }

        public static string GetClientWcfAddress()
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='wcfendpoint']");
            if (node != null)
            {
                return node.Attributes["address"].Value;
            }
            return null;
        }

        public static void SetClientWcfAddress(string url)
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='wcfendpoint']");
            if (node != null)
            {
                node.Attributes["address"].Value = url;
            }
        }

        public static string GetClientFileAddress()
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='fileendpoint']");
            if (node != null)
            {
                return node.Attributes["address"].Value;
            }
            return null;
        }

        public static void SetClientFileAddress(string url)
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='fileendpoint']");
            if (node != null)
            {
                node.Attributes["address"].Value = url;
            }
        }

        public static string GetClientLocalAddress()
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='localendpoint']");
            if (node != null)
            {
                return node.Attributes["address"].Value;
            }
            return null;
        }

        public static void SetClientLocalAddress(string url)
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='localendpoint']");
            if (node != null)
            {
                node.Attributes["address"].Value = url;
            }
        }

        //public static string GetWebapiAddress()
        //{
        //    if (xmldoc_webapi == null) InitConfig();
        //    XmlNode node = xmldoc_webapi.DocumentElement.SelectSingleNode("appSettings/add[@key='WebApiUri']");
        //    if (node != null)
        //    {
        //        return node.Attributes["value"].Value;
        //    }
        //    return null;
        //}

        //public static string GetWebapiClientBase()
        //{
        //    if (xmldoc_webapi == null) InitConfig();
        //    XmlNode node = xmldoc_webapi.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='wcfendpoint']");
        //    if (node != null)
        //    {
        //        return node.Attributes["address"].Value;
        //    }
        //    return null;
        //}

        //public static string GetWebapiClientFile()
        //{
        //    if (xmldoc_webapi == null) InitConfig();
        //    XmlNode node = xmldoc_webapi.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='fileendpoint']");
        //    if (node != null)
        //    {
        //        return node.Attributes["address"].Value;
        //    }
        //    return null;
        //}

        //public static void SetWebapiAddress(string url)
        //{
        //    if (xmldoc_webapi == null) InitConfig();
        //    XmlNode node = xmldoc_webapi.DocumentElement.SelectSingleNode("appSettings/add[@key='WebApiUri']");
        //    if (node != null)
        //    {
        //        node.Attributes["value"].Value = url;
        //    }
        //}

        //public static void SetWebapiClientBase(string url)
        //{
        //    if (xmldoc_webapi == null) InitConfig();
        //    XmlNode node = xmldoc_webapi.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='wcfendpoint']");
        //    if (node != null)
        //    {
        //        node.Attributes["address"].Value = url;
        //    }
        //}

        //public static void SetWebapiClientFile(string url)
        //{
        //    if (xmldoc_webapi == null) InitConfig();
        //    XmlNode node = xmldoc_webapi.DocumentElement.SelectSingleNode("system.serviceModel/client/endpoint[@name='fileendpoint']");
        //    if (node != null)
        //    {
        //        node.Attributes["address"].Value = url;
        //    }
        //}

        public static string GetUpdaterUrl()
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("appSettings/add[@key='UpdaterUrl']");
            if (node != null)
            {
                return node.Attributes["value"].Value;
            }
            return null;
        }

        public static void SetUpdaterUrl(string url)
        {
            if (xmldoc_app == null) InitConfig();
            XmlNode node = xmldoc_app.DocumentElement.SelectSingleNode("appSettings/add[@key='UpdaterUrl']");
            if (node != null)
            {
                node.Attributes["value"].Value = url;
            }
        }


        public static void SaveConfig()
        {
            if (xmldoc_app == null) InitConfig();
            xmldoc_app.Save(appconfig);
            InitConfig();
        }
    }
    /// <summary>
    /// 数据库配置
    /// </summary>
    public class HostDataBaseConfig
    {
        private static string entlibconfig = AppGlobal.AppRootPath + "Config\\EntLib.config";
        private static XmlDocument xmldoc_entlib;
        private static void InitConfig()
        {
            xmldoc_entlib = new XmlDocument();
            xmldoc_entlib.Load(entlibconfig);
        }

        public static string GetConnString()
        {
            if (xmldoc_entlib == null) InitConfig();
            XmlNode node = xmldoc_entlib.DocumentElement.SelectSingleNode("connectionStrings");
            if (node != null)
            {
                return node.InnerXml;
            }
            return null;
        }

        public static void SetConnString(string str)
        {
            if (xmldoc_entlib == null) InitConfig();
            XmlNode node = xmldoc_entlib.DocumentElement.SelectSingleNode("connectionStrings");
            if (node != null)
            {
                node.InnerXml = str;
            }
        }

        public static void SaveConfig()
        {
            if (xmldoc_entlib == null) InitConfig();
            xmldoc_entlib.Save(entlibconfig);
            InitConfig();
        }
    }
    /// <summary>
    /// 路由表配置
    /// </summary>
    public class HostRouterXml
    {
        private static string routerfile = System.Windows.Forms.Application.StartupPath + "\\Config\\RouterBill.xml";

        public static string GetXml()
        {
            FileInfo file = new FileInfo(routerfile);
            if (file.Exists)
            {
                return file.OpenText().ReadToEnd();
            }
            return null;
        }

        public static bool SaveXml(string xml)
        {
            FileInfo file = new FileInfo(routerfile);
            if (file.Exists)
            {
                file.Delete();
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(routerfile, true))
                {
                    sw.Write(xml);//直接追加文件末尾，不换行 
                }
                return true;
            }

            return false;
        }
    }
    /// <summary>
    /// MongoDB配置文件
    /// </summary>
    public class HostMongoDBConfig
    {
        private static string mongoconf = System.Windows.Forms.Application.StartupPath + "\\Config\\mongo.conf";
        private static string mongoconf_temp = System.Windows.Forms.Application.StartupPath + "\\Config\\mongo_temp.conf";

        public static string GetConfig()
        {
            string conf = null;
            FileInfo file = new FileInfo(mongoconf);
            if (file.Exists)
            {
                using (FileStream fsteam = file.OpenRead())
                {
                    byte[] buff = new byte[fsteam.Length];
                    fsteam.Read(buff, 0, buff.Length);
                    conf = Encoding.GetEncoding("gb2312").GetString(buff);
                }
            }
            return conf;
        }

        public static string GetConfig_Temp()
        {
            string conf = null;
            FileInfo file = new FileInfo(mongoconf_temp);
            if (file.Exists)
            {
                using (FileStream fsteam = file.OpenRead())
                {
                    byte[] buff = new byte[fsteam.Length];
                    fsteam.Read(buff, 0, buff.Length);
                    conf = Encoding.GetEncoding("gb2312").GetString(buff);
                }
            }
            return conf;
        }

        public static bool SetConfig(string conf)
        {
            FileInfo file = new FileInfo(mongoconf);
            if (file.Exists)
            {
                file.Delete();
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(mongoconf, true, Encoding.Default))
                {
                    sw.Write(conf);//直接追加文件末尾，不换行 
                }
                return true;
            }

            return false;
        }
    }
    /// <summary>
    /// 主机运行时配置信息
    /// </summary>
    public class HostRunConfigInfo
    {

        /// <summary>
        /// 基本参数
        /// </summary>
        public static HostRunConfigSubject BaseSubject { get; set; }

        /// <summary>
        /// 发布通讯地址
        /// </summary>
        public static HostRunConfigSubject IssueAddressSubject { get; set; }

        /// <summary>
        /// 数据库连接
        /// </summary>
        public static HostRunConfigSubject DataBaseSubject { get; set; }

        /// <summary>
        /// MongoDB配置
        /// </summary>
        public static HostRunConfigSubject MongoDBSubject { get; set; }

        /// <summary>
        /// 与上级通讯地址
        /// </summary>
        public static HostRunConfigSubject SuperiorAddressSubject { get; set; }

        /// <summary>
        /// 路由表
        /// </summary>
        public static HostRunConfigSubject RouterSubject { get; set; }

        /// <summary>
        /// 插件配置
        /// </summary>
        public static HostRunConfigSubject PluginSubject { get; set; }


        /// <summary>
        /// 发布订阅
        /// </summary>
        public static HostRunConfigSubject PublishSubject { get; set; }

        /// <summary>
        /// 订阅服务
        /// </summary>
        public static HostRunConfigSubject SubscribeSubject { get; set; }


        /// <summary>
        /// 定时任务
        /// </summary>
        public static HostRunConfigSubject TaskSubject { get; set; }

        private static Action<HostRunConfigSubject> pluginSubjectAction;
        private static Action<HostRunConfigSubject> publishSubjectAction;
        private static Action<HostRunConfigSubject> subscribeSubjectAction;
        private static Action<HostRunConfigSubject> taskSubjectAction;

        public static void LoadConfigInfo(string Identify, Action<HostRunConfigSubject> psAction, Action<HostRunConfigSubject> pubsAction, Action<HostRunConfigSubject> subsAction, Action<HostRunConfigSubject> taskAction)
        {
            pluginSubjectAction = psAction;
            publishSubjectAction = pubsAction;
            subscribeSubjectAction = subsAction;
            taskSubjectAction = taskAction;
            HostRunConfigObject configObj = null;

            #region 基本参数
            BaseSubject = new HostRunConfigSubject();
            BaseSubject.Title = "[基本参数]";
            BaseSubject.ConfigObjList = new List<HostRunConfigObject>();

            configObj = new HostRunConfigObject();
            configObj.Label = "中间件标识";
            configObj.Key = "Identify";
            configObj.Value = HostSettingConfig.GetValue("cdkey"); 
            configObj.Memo = Identify;
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "中间件名称";
            configObj.Key = "hostname";
            configObj.Value = HostSettingConfig.GetValue("hostname");
            configObj.Memo = HostSettingConfig.GetValue("hostname");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否显示调试信息";
            configObj.Key = "debug";
            configObj.Value = HostSettingConfig.GetValue("debug");
            configObj.Memo = (HostSettingConfig.GetValue("debug") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启定时任务";
            configObj.Key = "timingtask";
            configObj.Value = HostSettingConfig.GetValue("timingtask");
            configObj.Memo = (HostSettingConfig.GetValue("timingtask") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启基础服务";
            configObj.Key = "wcfservice";
            configObj.Value = HostSettingConfig.GetValue("wcfservice");
            configObj.Memo = (HostSettingConfig.GetValue("wcfservice") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启路由服务";
            configObj.Key = "router";
            configObj.Value = HostSettingConfig.GetValue("router");
            configObj.Memo = (HostSettingConfig.GetValue("router") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启文件传输";
            configObj.Key = "filetransfer";
            configObj.Value = HostSettingConfig.GetValue("filetransfer");
            configObj.Memo = (HostSettingConfig.GetValue("filetransfer") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启WebAPI";
            configObj.Key = "webapi";
            configObj.Value = HostSettingConfig.GetValue("webapi");
            configObj.Memo = (HostSettingConfig.GetValue("webapi") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启Nginx";
            configObj.Key = "nginx";
            configObj.Value = HostSettingConfig.GetValue("nginx");
            configObj.Memo = (HostSettingConfig.GetValue("nginx") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启MongoDB";
            configObj.Key = "mongodb";
            configObj.Value = HostSettingConfig.GetValue("mongodb");
            configObj.Memo = (HostSettingConfig.GetValue("mongodb") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            //configObj = new HostRunConfigObject();
            //configObj.Label = "MongoDB程序目录";
            //configObj.Key = "mongodb_binpath";
            //configObj.Value = HostSettingConfig.GetValue("mongodb_binpath");
            //configObj.Memo = HostSettingConfig.GetValue("mongodb_binpath");
            //BaseSubject.ConfigObjList.Add(configObj);

            //configObj = new HostRunConfigObject();
            //configObj.Label = "MongoDB连接字符串";
            //configObj.Key = "mongodb_conn";
            //configObj.Value = HostSettingConfig.GetValue("mongodb_conn");
            //configObj.Memo = HostSettingConfig.GetValue("mongodb_conn");
            //BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启心跳监测";
            configObj.Key = "heartbeat";
            configObj.Value = HostSettingConfig.GetValue("heartbeat");
            configObj.Memo = (HostSettingConfig.GetValue("heartbeat") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "心跳监测间隔时间";
            configObj.Key = "heartbeattime";
            configObj.Value = HostSettingConfig.GetValue("heartbeattime");
            configObj.Memo = HostSettingConfig.GetValue("heartbeattime") + "秒";
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启消息发送";
            configObj.Key = "message";
            configObj.Value = HostSettingConfig.GetValue("message");
            configObj.Memo = (HostSettingConfig.GetValue("message") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "消息发送间隔时间";
            configObj.Key = "messagetime";
            configObj.Value = HostSettingConfig.GetValue("messagetime");
            configObj.Memo = HostSettingConfig.GetValue("messagetime") + "秒";
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启日志记录";
            configObj.Key = "debug";
            configObj.Value = HostSettingConfig.GetValue("debug");
            configObj.Memo = (HostSettingConfig.GetValue("debug") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启数据压缩";
            configObj.Key = "compress";
            configObj.Value = HostSettingConfig.GetValue("compress");
            configObj.Memo = (HostSettingConfig.GetValue("compress") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否数据加密";
            configObj.Key = "encryption";
            configObj.Value = HostSettingConfig.GetValue("encryption");
            configObj.Memo = (HostSettingConfig.GetValue("encryption") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开启身份验证";
            configObj.Key = "token";
            configObj.Value = HostSettingConfig.GetValue("token");
            configObj.Memo = (HostSettingConfig.GetValue("token") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "数据序列化策略";
            configObj.Key = "serializetype";
            configObj.Value = HostSettingConfig.GetValue("serializetype");
            configObj.Memo = HostSettingConfig.GetValue("serializetype") == "0" ? "Newtonsoft" : (HostSettingConfig.GetValue("serializetype") == "1" ? "protobuf" : "fastJSON");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "是否开耗时日志记录";
            configObj.Key = "overtime";
            configObj.Value = HostSettingConfig.GetValue("overtime");
            configObj.Memo = (HostSettingConfig.GetValue("overtime") == "1" ? "开启" : "关闭");
            BaseSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "耗时日志超时时间";
            configObj.Key = "overtimetime";
            configObj.Value = HostSettingConfig.GetValue("overtimetime");
            configObj.Memo = HostSettingConfig.GetValue("overtimetime") + "秒";
            BaseSubject.ConfigObjList.Add(configObj);
            #endregion

            #region 发布通讯地址
            IssueAddressSubject = new HostRunConfigSubject();
            IssueAddressSubject.Title = "[发布通讯地址]";
            IssueAddressSubject.ConfigObjList = new List<HostRunConfigObject>();

            configObj = new HostRunConfigObject();
            configObj.Label = "基础数据服务地址";
            configObj.Key = "WcfAddress";
            configObj.Value = HostAddressConfig.GetWcfAddress();
            configObj.Memo = HostAddressConfig.GetWcfAddress();
            IssueAddressSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "文件传输服务地址";
            configObj.Key = "FileAddress";
            configObj.Value = HostAddressConfig.GetFileAddress();
            configObj.Memo = HostAddressConfig.GetFileAddress();
            IssueAddressSubject.ConfigObjList.Add(configObj);

            //configObj = new HostRunConfigObject();
            //configObj.Label = "路由基础服务地址";
            //configObj.Key = "RouterAddress";
            //configObj.Value = HostAddressConfig.GetRouterAddress();
            //configObj.Memo = HostAddressConfig.GetRouterAddress();
            //IssueAddressSubject.ConfigObjList.Add(configObj);

            //configObj = new HostRunConfigObject();
            //configObj.Label = "路由文件服务地址";
            //configObj.Key = "FileRouterAddress";
            //configObj.Value = HostAddressConfig.GetfileRouterAddress();
            //configObj.Memo = HostAddressConfig.GetfileRouterAddress();
            //IssueAddressSubject.ConfigObjList.Add(configObj);

            //configObj = new HostRunConfigObject();
            //configObj.Label = "WebAPI服务地址";
            //configObj.Key = "WebapiAddress";
            //configObj.Value = HostAddressConfig.GetWebapiAddress();
            //configObj.Memo = HostAddressConfig.GetWebapiAddress();
            //IssueAddressSubject.ConfigObjList.Add(configObj);
            #endregion

            #region 数据库连接
            DataBaseSubject = new HostRunConfigSubject();
            DataBaseSubject.Title = "[数据库连接]";
            DataBaseSubject.ConfigObjList = new List<HostRunConfigObject>();

            configObj = new HostRunConfigObject();
            configObj.Label = "连接字符串";
            configObj.Key = "ConnString";
            configObj.Value = HostDataBaseConfig.GetConnString();
            configObj.Memo = HostDataBaseConfig.GetConnString();
            DataBaseSubject.ConfigObjList.Add(configObj);
            #endregion

            #region MongoDB配置
            MongoDBSubject = new HostRunConfigSubject();
            MongoDBSubject.Title = "[MongoDB配置]";
            MongoDBSubject.ConfigObjList = new List<HostRunConfigObject>();

            configObj = new HostRunConfigObject();
            configObj.Label = "MongoDB程序目录";
            configObj.Key = "mongodb_binpath";
            configObj.Value = HostSettingConfig.GetValue("mongodb_binpath");
            configObj.Memo = HostSettingConfig.GetValue("mongodb_binpath");
            MongoDBSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "MongoDB连接字符串";
            configObj.Key = "mongodb_conn";
            configObj.Value = HostSettingConfig.GetValue("mongodb_conn");
            configObj.Memo = HostSettingConfig.GetValue("mongodb_conn");
            MongoDBSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "MongoDB配置文件";
            configObj.Key = "mongo_config";
            configObj.Value = HostMongoDBConfig.GetConfig();
            configObj.Memo = "\r" + HostMongoDBConfig.GetConfig();
            MongoDBSubject.ConfigObjList.Add(configObj);
            #endregion

            #region 与上级通讯地址
            SuperiorAddressSubject = new HostRunConfigSubject();
            SuperiorAddressSubject.Title = "[与上级通讯地址]";
            SuperiorAddressSubject.ConfigObjList = new List<HostRunConfigObject>();

            configObj = new HostRunConfigObject();
            configObj.Label = "上级中间件业务请求地址";
            configObj.Key = "ClientWcfAddress";
            configObj.Value = HostAddressConfig.GetClientWcfAddress();
            configObj.Memo = HostAddressConfig.GetClientWcfAddress();
            SuperiorAddressSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "上级中间件文件传输地址";
            configObj.Key = "ClientFileAddress";
            configObj.Value = HostAddressConfig.GetClientFileAddress();
            configObj.Memo = HostAddressConfig.GetClientFileAddress();
            SuperiorAddressSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "本地中间件业务请求地址";
            configObj.Key = "ClientLocalAddress";
            configObj.Value = HostAddressConfig.GetClientLocalAddress();
            configObj.Memo = HostAddressConfig.GetClientLocalAddress();
            SuperiorAddressSubject.ConfigObjList.Add(configObj);

            configObj = new HostRunConfigObject();
            configObj.Label = "中间件升级地址";
            configObj.Key = "UpdaterUrl";
            configObj.Value = HostAddressConfig.GetUpdaterUrl();
            configObj.Memo = HostAddressConfig.GetUpdaterUrl();
            SuperiorAddressSubject.ConfigObjList.Add(configObj);
            #endregion

            #region 路由表
            //RouterSubject = new HostRunConfigSubject();
            //RouterSubject.Title = "[路由表]";
            //RouterSubject.ConfigObjList = new List<HostRunConfigObject>();

            //configObj = new HostRunConfigObject();
            //configObj.Label = "路由配置内容";
            //configObj.Key = "RouterXml";
            //configObj.Value = HostRouterXml.GetXml();
            //configObj.Memo ="\r"+ HostRouterXml.GetXml();
            //RouterSubject.ConfigObjList.Add(configObj);
            #endregion

            #region 插件配置
            PluginSubject = new HostRunConfigSubject();
            PluginSubject.Title = "[插件配置]";
            PluginSubject.ConfigObjList = new List<HostRunConfigObject>();

            if (AppPluginManage.PluginDic != null)
            {
                foreach (var p in AppPluginManage.PluginDic)
                {
                    configObj = new HostRunConfigObject();
                    configObj.Label = "本地插件";
                    configObj.Key = p.Key;
                    configObj.Value = p.Value.plugin.title;
                    configObj.Memo = p.Key + "\t" + p.Value.plugin.title + "@" + p.Value.plugin.version;
                    PluginSubject.ConfigObjList.Add(configObj);
                }
            }
            //远程插件
            if(pluginSubjectAction!=null)
            {
                pluginSubjectAction(PluginSubject);
            }
            #endregion

            #region 发布订阅
            PublishSubject = new HostRunConfigSubject();
            PublishSubject.Title = "[发布订阅]";
            PublishSubject.ConfigObjList = new List<HostRunConfigObject>();

            if (publishSubjectAction != null)
            {
                publishSubjectAction(PublishSubject);
            }
            #endregion

            #region 订阅服务
            SubscribeSubject = new HostRunConfigSubject();
            SubscribeSubject.Title = "[订阅服务]";
            SubscribeSubject.ConfigObjList = new List<HostRunConfigObject>();

            if (subscribeSubjectAction != null)
            {
                subscribeSubjectAction(SubscribeSubject);
            }
            #endregion

            #region 定时任务
            TaskSubject = new HostRunConfigSubject();
            TaskSubject.Title = "[定时任务]";
            TaskSubject.ConfigObjList = new List<HostRunConfigObject>();

            if (taskSubjectAction != null)
            {
                taskSubjectAction(TaskSubject);
            }
            #endregion
        }

        public static string GetConfigText()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine(HostRunConfigInfo.BaseSubject.Title);
            foreach(var i in HostRunConfigInfo.BaseSubject.ConfigObjList)
            {
                text.AppendLine(i.Label + ":\t\t" + i.Memo);
            }
            text.AppendLine();

            text.AppendLine(HostRunConfigInfo.DataBaseSubject.Title);
            foreach (var i in HostRunConfigInfo.DataBaseSubject.ConfigObjList)
            {
                text.AppendLine(i.Label + ":\t\t" + i.Memo);
            }
            text.AppendLine();

            text.AppendLine(HostRunConfigInfo.IssueAddressSubject.Title);
            foreach (var i in HostRunConfigInfo.IssueAddressSubject.ConfigObjList)
            {
                text.AppendLine(i.Label + ":\t\t" + i.Memo);
            }
            text.AppendLine();

            text.AppendLine(HostRunConfigInfo.MongoDBSubject.Title);
            foreach (var i in HostRunConfigInfo.MongoDBSubject.ConfigObjList)
            {
                text.AppendLine(i.Label + ":\t\t" + i.Memo);
            }
            text.AppendLine();

            text.AppendLine(HostRunConfigInfo.SuperiorAddressSubject.Title);
            foreach (var i in HostRunConfigInfo.SuperiorAddressSubject.ConfigObjList)
            {
                text.AppendLine(i.Label + ":\t\t" + i.Memo);
            }
            text.AppendLine();

            //text.AppendLine(HostRunConfigInfo.RouterSubject.Title);
            //foreach (var i in HostRunConfigInfo.RouterSubject.ConfigObjList)
            //{
            //    text.AppendLine(i.Label + ":\t\t" + i.Memo);
            //}
            //text.AppendLine();

            text.AppendLine(HostRunConfigInfo.PluginSubject.Title);
            foreach (var i in HostRunConfigInfo.PluginSubject.ConfigObjList)
            {
                text.AppendLine(i.Label + ":\t\t" + i.Memo);
            }
            text.AppendLine();

            text.AppendLine(HostRunConfigInfo.PublishSubject.Title);
            foreach (var i in HostRunConfigInfo.PublishSubject.ConfigObjList)
            {
                text.AppendLine(i.Label + ":\t\t" + i.Memo);
            }
            text.AppendLine();

            text.AppendLine(HostRunConfigInfo.SubscribeSubject.Title);
            foreach (var i in HostRunConfigInfo.SubscribeSubject.ConfigObjList)
            {
                text.AppendLine(i.Label + ":\t\t" + i.Memo);
            }
            text.AppendLine();

            text.AppendLine(HostRunConfigInfo.TaskSubject.Title);
            foreach (var i in HostRunConfigInfo.TaskSubject.ConfigObjList)
            {
                text.AppendLine(i.Label + ":\t\t" + i.Memo);
            }
            text.AppendLine();
            return text.ToString();
        }

        public static string GetConfigSubject()
        {
            List<HostRunConfigSubject> subjectList = new List<HostRunConfigSubject>();
            subjectList.Add(HostRunConfigInfo.BaseSubject);
            subjectList.Add(HostRunConfigInfo.DataBaseSubject);
            subjectList.Add(HostRunConfigInfo.IssueAddressSubject);
            subjectList.Add(HostRunConfigInfo.MongoDBSubject);
            subjectList.Add(HostRunConfigInfo.SuperiorAddressSubject);
            subjectList.Add(HostRunConfigInfo.RouterSubject);
            subjectList.Add(HostRunConfigInfo.PluginSubject);
            subjectList.Add(HostRunConfigInfo.PublishSubject);
            subjectList.Add(HostRunConfigInfo.SubscribeSubject);
            subjectList.Add(HostRunConfigInfo.TaskSubject);
            return JsonConvert.SerializeObject(subjectList);
        }
    }
    /// <summary>
    /// 配置主题
    /// </summary>
    public class HostRunConfigSubject
    {
        public string Title { get; set; }
        public List<HostRunConfigObject> ConfigObjList { get; set; }
    }
    /// <summary>
    /// 配置对象
    /// </summary>
    public class HostRunConfigObject
    {
        public string Label { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Memo { get; set; }
    }
}
