using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Init;
using System.Xml;

namespace EFWCoreLib.CoreFrame.Plugin
{
    /// <summary>
    /// pluginsys.xml配置
    /// </summary>
    public class PluginSysManage
    {
        private static System.Xml.XmlDocument xmlDoc = null;
        private static string pluginsysFile = null;

        private static void InitConfig()
        {
            pluginsysFile = AppGlobal.AppRootPath + "Config\\pluginsys.xml";
            xmlDoc = new System.Xml.XmlDocument();

            //XmlReaderSettings settings = new XmlReaderSettings();
            //settings.IgnoreComments = true;//忽略文档里面的注释
            //XmlReader reader = XmlReader.Create(pluginsysFile, settings);
            //xmlDoc.Load(reader);
            //reader.Close();

            xmlDoc.Load(pluginsysFile);
        }

        /// <summary>
        /// 获取所有插件路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllPluginFile()
        {
            List<string> pflist = new List<string>();
            if (xmlDoc == null) InitConfig();
            XmlNodeList nl = xmlDoc.DocumentElement.SelectNodes("WcfModulePlugin/Plugin");
            foreach (XmlNode n in nl)
            {
                pflist.Add(n.Attributes["path"].Value);
            }
            return pflist;
        }

        /// <summary>
        /// 根据插件名获取插件路径
        /// </summary>
        /// <param name="pluginname"></param>
        /// <returns></returns>
        public static string GetPluginFile(string pluginname)
        {
            string pluginpath = null;
            if (xmlDoc == null) InitConfig();
            XmlNode xn = xmlDoc.DocumentElement.SelectSingleNode("WcfModulePlugin/Plugin[@name=" + pluginname + "]");
            if (xn != null)
            {
                pluginpath = xn.Attributes["path"].Value;
            }
            return pluginpath;
        }

        public static bool ContainPlugin(string name)
        {
            if (xmlDoc == null) InitConfig();
            XmlNode xn = xmlDoc.DocumentElement.SelectSingleNode("WcfModulePlugin/Plugin[@name='" + name + "']");
            if (xn == null)
                return false;
            else
                return true;
        }

        public static void AddPlugin(string pluginfile)
        {
            string filepath = AppGlobal.AppRootPath + pluginfile;//转为绝对路径
            if (System.IO.File.Exists(filepath))
            {
                XmlDocument xml_plugin = new System.Xml.XmlDocument();
                xml_plugin.Load(filepath);
                XmlNode node_plugin= xml_plugin.DocumentElement.SelectSingleNode("plugin");
                AddPlugin(node_plugin.Attributes["name"].Value, node_plugin.Attributes["title"].Value, pluginfile, node_plugin.Attributes["version"].Value);
            }
        }

        public static void AddPlugin(string name, string title, string path, string version)
        {
            if (xmlDoc == null) InitConfig();
            if (ContainPlugin(name) == false)
            {
                XmlNode root = xmlDoc.DocumentElement.SelectSingleNode("WcfModulePlugin");//查找   
                XmlElement xe1 = xmlDoc.CreateElement("Plugin");//创建一个节点   
                xe1.SetAttribute("name", name);//
                xe1.SetAttribute("path", path);//
                xe1.SetAttribute("title", title);//
                xe1.SetAttribute("version", version);//版本

                root.AppendChild(xe1);//添加到<bookstore>节点中   
                xmlDoc.Save(pluginsysFile);
            }
            else
            {
                XmlNode xn = xmlDoc.DocumentElement.SelectSingleNode("WcfModulePlugin/Plugin[@name='" + name + "']");
                xn.Attributes["name"].Value = name;
                xn.Attributes["path"].Value = path;
                xn.Attributes["title"].Value = title;
                xn.Attributes["version"].Value = version;
                xmlDoc.Save(pluginsysFile);
            }
        }

        public static void RemovePlugin(string name)
        {
            if (xmlDoc == null) InitConfig();
            XmlNode xn = xmlDoc.DocumentElement.SelectSingleNode("WcfModulePlugin/Plugin[@name='" + name + "']");
            if (xn != null)
                xn.ParentNode.RemoveChild(xn);
            xmlDoc.Save(pluginsysFile);
        }
    }
}
