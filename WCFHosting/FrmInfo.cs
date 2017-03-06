using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.CoreFrame.ProcessManage;

namespace WCFHosting
{
    public partial class FrmInfo : Form
    {
        public FrmInfo()
        {
            InitializeComponent();
        }

        private void FrmInfo_Load(object sender, EventArgs e)
        {
            txtInfo.Text = Program.serverIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "getmnodetext", null);
        }
    }
}
