using EFWCoreLib.CoreFrame.Init;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EFWCoreLib.CoreFrame.Plugin
{
    /// <summary>
    /// 插件配置文件数据
    /// </summary>
    public class PluginConfig : MarshalByRefObject
    {
        public string name { get; set; }
        public string version { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public string plugintype { get; set; }
        public string defaultdbkey { get; set; }
        public string defaultcachekey { get; set; }

        public List<baseinfoData> baseinfoDataList { get; set; }

        public void Load(PluginSectionHandler plugin, string plugfile)
        {
            if (plugin != null)
            {
                if (baseinfoDataList == null) baseinfoDataList = new List<baseinfoData>();

                name = plugin.name;
                version = plugin.version;
                title = plugin.title;
                author = plugin.author;
                plugintype = plugin.plugintype;
                defaultdbkey = plugin.defaultdbkey;
                defaultcachekey = plugin.defaultcachekey;

                foreach (baseinfoData data in plugin.baseinfo)
                {
                    if (baseinfoDataList.FindIndex(x => x.key == data.key) == -1)
                        baseinfoDataList.Add(data);
                }
            }
        }

        public string GetBaseInfoValue(string key)
        {
            baseinfoData data= baseinfoDataList.Find(x => x.key == key);
            if (data != null)
            {
                return data.value;
            }

            return null;
        }
    }
}
