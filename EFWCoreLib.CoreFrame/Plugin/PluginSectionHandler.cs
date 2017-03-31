using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace EFWCoreLib.CoreFrame.Plugin
{


    public class PluginSectionHandler:ConfigurationSection
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public String name
        {
            get
            { return (String)this["name"]; }
            set
            { this["name"] = value; }
        }

        [ConfigurationProperty("version", IsRequired = true)]
        public String version
        {
            get
            { return (String)this["version"]; }
            set
            { this["version"] = value; }
        }

        [ConfigurationProperty("title", IsRequired = true)]
        public String title
        {
            get
            { return (String)this["title"]; }
            set
            { this["title"] = value; }
        }

        [ConfigurationProperty("author", IsRequired = true)]
        public String author
        {
            get
            { return (String)this["author"]; }
            set
            { this["author"] = value; }
        }

        [ConfigurationProperty("plugintype", IsRequired = true)]
        public String plugintype
        {
            get
            { return (String)this["plugintype"]; }
            set
            { this["plugintype"] = value; }
        }

        [ConfigurationProperty("defaultdbkey", IsRequired = true)]
        public String defaultdbkey
        {
            get
            { return (String)this["defaultdbkey"]; }
            set
            { this["defaultdbkey"] = value; }
        }

        [ConfigurationProperty("defaultcachekey", IsRequired = true)]
        public String defaultcachekey
        {
            get
            { return (String)this["defaultcachekey"]; }
            set
            { this["defaultcachekey"] = value; }
        }

        [ConfigurationProperty("isentry")]
        public string isentry
        {
            get
            { return (String)this["isentry"]; }
            set
            { this["isentry"] = value; }
        }

        [ConfigurationProperty("baseinfo", IsDefaultCollection = true)]
        public BaseInfoCollection baseinfo
        {
            get
            {
                return (BaseInfoCollection)base["baseinfo"];
            }
        }
    }

    public class BaseInfoCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new baseinfoData();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((baseinfoData)element).key;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "data";
            }
        }

        public baseinfoData this[int index]
        {
            get
            {
                return (baseinfoData)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);

            }
        }
    }


    public class baseinfoData : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public string key
        {
            get
            {
                return (string)base["key"];
            }
            set
            {
                base["key"] = value;
            }
        }
        [ConfigurationProperty("value")]
        public string value
        {
            get
            {
                return (string)base["value"];
            }
            set
            {
                base["value"] = value;
            }
        }
    }



}
