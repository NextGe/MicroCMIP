using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.WebApiFrame;

namespace EFWCoreLib.WebFrame.WebAPI
{
    public class PluginAssembliesResolver : DefaultAssembliesResolver
    {
        #region IAssembliesResolver 成员

        public override ICollection<System.Reflection.Assembly> GetAssemblies()
        {
            List<System.Reflection.Assembly> list = new List<System.Reflection.Assembly>();
            string[] dlls = GetPluginDll();
            foreach (var file in dlls)
            {
                list.Add(System.Reflection.Assembly.Load(file));
            }
            list.Add(System.Reflection.Assembly.Load("EFWCoreLib.CoreFrame"));
            list.Add(System.Reflection.Assembly.Load("EFWCoreLib.WinformFrame"));
            list.Add(System.Reflection.Assembly.Load("EFWCoreLib.WcfFrame"));
            list.Add(System.Reflection.Assembly.Load("EFWCoreLib.WebApiFrame"));
            return list;
        }

        #endregion


        private string[] GetPluginDll()
        {
            List<string> dlllist = new List<string>();
            foreach (var p in WebApiGlobal.PluginPath)
            {
                DirectoryInfo Dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory+p);
                FileInfo[] dlls = Dir.GetFiles("*.dll", SearchOption.AllDirectories);
                foreach (var i in dlls)
                {
                    dlllist.Add(i.Name.Replace(".dll", ""));
                }
            }
            return dlllist.ToArray();
        }
        
    }
}
