using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFWCoreLib.WebAPI.Utility
{
    /// <summary>
    /// 树控件数据结构
    /// </summary>
    public class amazeuitreenode
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<amazeuitreenode> childs { get; set; }
        public Dictionary<string, string> attr { get; set; }
    }


}
