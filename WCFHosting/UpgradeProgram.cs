using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.CoreFrame.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace efwplusHosting
{
    /// <summary>
    /// 升级程序
    /// </summary>
    public static class UpgradeProgram
    {
        static string rootpath = System.Windows.Forms.Application.StartupPath + "\\";
        //安装插件升级包
        public static void SetUpPluginUpgrade()
        {
            string pufile = System.Windows.Forms.Application.StartupPath + "\\pluginupgrade.txt";

            if (File.Exists(pufile) == true)
            {
                List<string> addplugin = new List<string>();//新增插件
                List<string> updateplugin = new List<string>();//更新插件
                List<string> deleteplugin = new List<string>();//删除插件

                using (StreamReader sr = new StreamReader(pufile))
                {
                    string addrow = sr.ReadLine();
                    addplugin = addrow.Split(':')[1].Split(',').ToList();

                    string updaterow = sr.ReadLine();
                    updateplugin = updaterow.Split(':')[1].Split(',').ToList();

                    string deleterow = sr.ReadLine();
                    deleteplugin = deleterow.Split(':')[1].Split(',').ToList();
                }

                //删除
                File.Delete(pufile);

                foreach (string p in addplugin)
                {
                    if (p.Trim() != "")
                    {
                        string path = rootpath + "ModulePlugin\\" + p;
                        //删除本地插件
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        //解压插件包
                        string zipfile = rootpath + @"FileStore\PluginUpgrade\" + p + ".zip";
                        FastZipHelper.decompress(rootpath + "ModulePlugin\\", zipfile);
                        //修改pluginsys.xml配置文件
                        string pluginfile = "ModulePlugin\\" + p + "\\plugin.xml";
                        PluginSysManage.AddPlugin(pluginfile);
                    }
                }

                foreach (string p in updateplugin)
                {
                    if (p.Trim() != "")
                    {
                        string path = rootpath + "ModulePlugin\\" + p;
                        //删除本地插件
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        //解压插件包
                        string zipfile = rootpath + @"FileStore\PluginUpgrade\" + p + ".zip";
                        FastZipHelper.decompress(rootpath + "ModulePlugin\\", zipfile);
                        //修改pluginsys.xml配置文件
                        string pluginfile = "ModulePlugin\\" + p + "\\plugin.xml";
                        PluginSysManage.RemovePlugin(p);
                        PluginSysManage.AddPlugin(pluginfile);
                    }
                }

                foreach (string p in deleteplugin)
                {
                    if (p.Trim() != "")
                    {
                        string path = rootpath + "ModulePlugin\\" + p;
                        //修改pluginsys.xml配置文件
                        PluginSysManage.RemovePlugin(p);
                        //删除本地插件
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        
                    }
                }
            }
        }
    }
}
