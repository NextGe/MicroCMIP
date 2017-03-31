using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.CoreFrame.Init.AttributeManager;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using EFWCoreLib.CoreFrame.Plugin;
using System.Reflection;

namespace EFWCoreLib.CoreFrame.Init
{
    /// <summary>
    /// 程序运行后的插件管理
    /// 插件实现热插拔，有两种方式：
    /// 1）用AppDomain，但这种方式在这里行不通，因为AppDomain之间传递的对象太复杂
    /// 2）用内存中读取dll的方式
    /// </summary>
    public class AppPluginManage
    {
        public static Dictionary<string, ModulePlugin> PluginDic;//本地插件
        /// <summary>
        /// 加载所有插件
        /// </summary>
        public static void LoadAllPlugin()
        {
            PluginDic = new Dictionary<string, ModulePlugin>();
            List<string> pflist = PluginSysManage.GetAllPluginFile();
            for (int i = 0; i < pflist.Count; i++)
            {
                //AddPlugin(pflist[i]);
                string filepath = AppGlobal.AppRootPath + pflist[i];//转为绝对路径
                ModulePlugin mp = new ModulePlugin();
                mp.appType = AppGlobal.appType;
                mp.LoadPlugin(filepath);
                mp.LoadAttribute(filepath);
                PluginDic.Add(mp.plugin.name, mp);
            }
        }

        /// <summary>
        /// 加载插件,之后必须重启中间件
        /// </summary>
        /// <param name="plugfile"></param>
        public static void AddPlugin(string pluginfile)
        {
            string filepath = AppGlobal.AppRootPath + pluginfile;//转为绝对路径

            ModulePlugin mp = new ModulePlugin();
            mp.appType = AppGlobal.appType;
            mp.LoadPlugin(filepath);

            PluginSysManage.AddPlugin(mp.plugin.name, mp.plugin.title, pluginfile, mp.plugin.version);
        }
        /// <summary>
        /// 卸载插件,之后必须重启中间件
        /// </summary>
        /// <param name="plugname"></param>
        public static void RemovePlugin(string pluginname)
        {
            PluginSysManage.RemovePlugin(pluginname);
        }

        
        public static WcfControllerAttributeInfo GetPluginWcfControllerAttributeInfo(string pluginname, string name, out ModulePlugin mp)
        {
            mp = PluginDic[pluginname];
            if (mp != null)
            {
                List<WcfControllerAttributeInfo> list = (List<WcfControllerAttributeInfo>)WcfControllerManager.GetAttributeInfo(mp.cache, mp.plugin.name);
                if (list.FindIndex(x => x.controllerName == name) > -1)
                {
                    return list.Find(x => x.controllerName == name);
                }
            }
            return null;
        }

        public static string GetBaseInfoDataValue(string _pluginName,string key)
        {
            return AppPluginManage.PluginDic[_pluginName].plugin.GetBaseInfoValue(key);
        }
    }

    
}
