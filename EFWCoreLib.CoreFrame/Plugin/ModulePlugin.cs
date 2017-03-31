using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using EFWCoreLib.CoreFrame.DbProvider;
using EFWCoreLib.CoreFrame.EntLib;
using System.Configuration;
using Microsoft.Practices.Unity.Configuration;
using System.Reflection;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.CoreFrame.Init.AttributeManager;
using EFWCoreLib.CoreFrame.Business;
using System.IO;

namespace EFWCoreLib.CoreFrame.Plugin
{
    /// <summary>
    /// 模块插件
    /// </summary>
    public class ModulePlugin : MarshalByRefObject
    {
        /// <summary>
        /// 插件配置
        /// </summary>
        public PluginConfig plugin { get; set; }
        /// <summary>
        /// 数据库对象
        /// </summary>
        public AbstractDatabase database{get;set;}
        /// <summary>
        /// Unity对象容器
        /// </summary>
        public IUnityContainer container{get;set;}
        /// <summary>
        /// 企业库缓存
        /// </summary>
        public ICacheManager cache{get;set;}

        /// <summary>
        /// 执行控制器
        /// </summary>
        public AbstractControllerHelper helper { get; set; }

        public AppType appType { get; set; }

        public List<Assembly> DllList { get; set; }


        public ModulePlugin()
        {
           
            
        }

        private List<Assembly> LoadDllList(string plugfile)
        {
            List<Assembly> dllList = new List<Assembly>();
            FileInfo[] dlls = (new System.IO.FileInfo(plugfile).Directory).GetFiles("*.dll", SearchOption.AllDirectories);
            foreach (var i in dlls)
            {
                dllList.Add(Assembly.Load(i.Name.Replace(".dll", "")));
            }

            return dllList;
        }

        /// <summary>
        /// 导入插件配置文件
        /// </summary>
        /// <param name="plugfile">插件配置文件路径</param>
        public void LoadPlugin(string plugfile)
        {
            

            container = ZhyContainer.CreateUnity();
            plugin = new PluginConfig();

            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = plugfile };
            System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            var plugininfo = (PluginSectionHandler)configuration.GetSection("plugin");
            if (plugininfo != null)
                plugin.Load(plugininfo, plugfile);

            DllList = LoadDllList(plugfile);//加载程序集

            var unitySection = (UnityConfigurationSection)configuration.GetSection("unity");
            if (unitySection != null)
                container.LoadConfiguration(unitySection);//判断EntLib的路径对不对

            

            if (plugin.defaultdbkey != "")
                database = FactoryDatabase.GetDatabase(plugin.defaultdbkey);
            else
                database = FactoryDatabase.GetDatabase();

            database.PluginName = plugin.name;

            if (plugin.defaultcachekey != "")
                cache = ZhyContainer.CreateCache(plugin.defaultcachekey);
            else
                cache = ZhyContainer.CreateCache();
        }

        public void LoadAttribute(string plugfile)
        {
            if (DllList.Count > 0)
            {
                EntityManager.LoadAttribute(DllList, cache, plugin.name);
                WcfControllerManager.LoadAttribute(DllList, this);
            }
        }

        public void Remove()
        {
            ICacheManager _cache = cache;
            EntityManager.ClearAttributeData(_cache, plugin.name);
            WcfControllerManager.ClearAttributeData(_cache, plugin.name);
        }
    }
}
