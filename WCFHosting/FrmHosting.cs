using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.CoreFrame.ProcessManage;

namespace WCFHosting
{
    public partial class FrmHosting : Form
    {
        Func<string, Dictionary<string,string>, string> ExecCmd;
        long timeCount = 0;//运行次数

        HostState RunState
        {
            set
            {
                if (value == HostState.NoOpen)
                {
                    btnStart.Enabled = true;
                    btnStop.Enabled = false;
                    启动ToolStripMenuItem.Enabled = true;
                    停止ToolStripMenuItem.Enabled = false;

                    lbStatus.Text = "服务未启动";
                    timerRun.Enabled = false;
                    //timermsg.Enabled = false;
                    //暂停日志ToolStripMenuItem.Text = "开启日志";
                }
                else
                {
                    btnStart.Enabled = false;
                    btnStop.Enabled = true;
                    启动ToolStripMenuItem.Enabled = false;
                    停止ToolStripMenuItem.Enabled = true;

                    lbStatus.Text = "服务已运行";
                    timeCount = 0;
                    timerRun.Enabled = true;
                    //timermsg.Enabled = true;
                    //暂停日志ToolStripMenuItem.Text = "暂停日志";
                }
            }
        }

        public FrmHosting(Func<string, Dictionary<string,string>, string> _execCmd)
        {
            ExecCmd = _execCmd;
            InitializeComponent();
            //msgList = new Queue<msgobject>();
        }

        


        private void FrmHosting_Load(object sender, EventArgs e)
        {
            tabMain.TabPages.RemoveAt(2);
            tabMain.TabPages.RemoveAt(1);
            

            this.Text = "MicroCMIP 微云医疗信息平台【" + HostSettingConfig.GetValue("hostname") + "】";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.Icon = this.Icon;
            this.notifyIcon1.Text = this.Text;

            RunState = HostState.NoOpen;
            lsServerUrl.Text = "http://localhost:8088/Monitor/index.html";
            btnStart_Click(null, null);//打开服务主机后自动启动服务
        }

        #region 显示信息
        public delegate void textInvoke(Color clr, string msg);
        public delegate void gridInvoke(DataGridView grid, object data);
        private void settext(Color clr, string msg)
        {
            if (richTextMsg.InvokeRequired)
            {
                textInvoke ti = new textInvoke(settext);
                this.BeginInvoke(ti, new object[] { clr, msg });
            }
            else
            {
                ListViewItem lstItem = new ListViewItem(msg);
                lstItem.ForeColor = clr;
                if (richTextMsg.Items.Count > 1000)
                    richTextMsg.Items.Clear();
                richTextMsg.Items.Add(lstItem);
                richTextMsg.SelectedIndex = richTextMsg.Items.Count - 1;
            }
        }
        public void showmsg(string msg)
        {
            settext(Color.Black,msg);
        }
        private void setgrid(DataGridView grid, object data)
        {
            if (grid.InvokeRequired)
            {
                gridInvoke gi = new gridInvoke(setgrid);
                this.BeginInvoke(gi, new object[] { grid, data });
            }
            else
            {
                grid.AutoGenerateColumns = false;
                grid.DataSource = data;
                grid.Refresh();
            }
        }

        //private void BindGridClient(List<ClientInfo> dic)
        //{
        //    setgrid(gridClientList, dic);
        //}
        //private void BindGridRouter(List<RegistrationInfo> dic)
        //{
        //    setgrid(gridRouter, dic);
        //}
        //显示日志
        private void richTextMsg_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            ListViewItem lstItem = (ListViewItem)richTextMsg.Items[e.Index];
            e.DrawBackground();
            Brush brsh = Brushes.White;
            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
                brsh = new SolidBrush(lstItem.ForeColor);
            String sText = lstItem.Text.Replace('\n', ' ');
            SizeF sz = e.Graphics.MeasureString(sText, e.Font, new SizeF(e.Bounds.Width, e.Bounds.Height));
            e.Graphics.DrawString(sText, e.Font, brsh, e.Bounds.Left, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 0.5f);
        }
        //运行时间显示
        private void timer1_Tick(object sender, EventArgs e)
        {
            timeCount++;
            //显示运行时间
            long iHour = timeCount / 3600;
            long iMin = (timeCount % 3600) / 60;
            long iSec = timeCount % 60;
            if (iHour > 23)
                lbRunTime.Text = String.Format("{0}天 {1:02d}:{2:0#}:{3:0#}", iHour / 24, iHour % 24, iMin, iSec);
            else
                lbRunTime.Text = String.Format("{0:0#}:{1:0#}:{2:0#}", iHour, iMin, iSec);

            lbClientCount.Text = gridClientList.RowCount.ToString();
        }

        #endregion

        //启动
        private void btnStart_Click(object sender, EventArgs e)
        {
            ExecCmd("quitall", null);
            ExecCmd("startall", null);

            RunState = HostState.Opened;
        }
        //停止
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("您确定要停止服务吗？", "询问窗", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                ExecCmd("quitall", null);

                RunState = HostState.NoOpen;
            }
        }

        //启动
        private void 启动ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnStart_Click(null, null);
        }
        //停止
        private void 停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnStop_Click(null, null);
        }

        //显示
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }
        //显示
        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }
        
        //退出
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("您确定要退出中间件服务器吗？", "询问窗", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                try
                {
                    ExecCmd("quitall", null);

                    RunState = HostState.NoOpen;
                }
                catch { }
                this.notifyIcon1.Dispose();
                //System.Environment.Exit(System.Environment.ExitCode);
                Process.GetCurrentProcess().Kill();
            }
        }
        //退出
        private void FrmHosting_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        //综合信息
        private void btnInfo_Click(object sender, EventArgs e)
        {
            FrmInfo info = new FrmInfo();
            info.ShowDialog();
        }
        //一键重启
        private void btnrestart_Click(object sender, EventArgs e)
        {


            if (MessageBox.Show("您确定要重启中间件服务器吗？", "询问窗", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                try
                {
                    //MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Red, "正在准备重启中间件服务，请等待...");
                    efwplusBaseManager.StopBase();
                    efwplusRouteManager.StopRoute();
                    efwplusWebAPIManager.StopAPI();
                    MongodbManager.StopDB();
                    NginxManager.StopWeb();

                    RunState = HostState.NoOpen;

                    efwplusBaseManager.StartBase();
                    efwplusRouteManager.StartRoute();
                    efwplusWebAPIManager.StartAPI();
                    MongodbManager.StartDB();
                    NginxManager.StartWeb();

                    RunState = HostState.Opened;
                }
                catch { }
            }
        }

        //设置
        private void btnSetting_Click(object sender, EventArgs e)
        {
            FrmSetting frmsetting = new FrmSetting();
            frmsetting.ShowDialog();
        }
        private void 清除日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //msgList.Clear();
            richTextMsg.Items.Clear();
        }

        private void 复制日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextMsg.SelectedItem == null)
                return;
            StringBuilder strMessage = new StringBuilder();
            for (int i = 0; i < richTextMsg.Items.Count; i++)
            {
                if (richTextMsg.GetSelected(i))
                    strMessage.Append(richTextMsg.SelectedItem.ToString());
            }

            Clipboard.SetDataObject(strMessage.ToString());
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.efwplus.cn");
        }

        private void 注册ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCDKEY cdkey = new frmCDKEY();
            cdkey.ShowDialog();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void lsServerUrl_Click(object sender, EventArgs e)
        {
            Process.Start(this.lsServerUrl.Text);
        }
    }

    public enum HostState
    {
        NoOpen,Opened
    }

    /// <summary>
    /// 连接客户端信息
    /// </summary>
    public class ClientInfo 
    {
        public string clientId { get; set; }
        public string clientName { get; set; }
        public DateTime startTime { get; set; }
        public int HeartbeatCount { get; set; }
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnect { get; set; }
        /// <summary>
        /// 请求次数
        /// </summary>
        public int RequestCount { get; set; }
        /// <summary>
        /// 接收数据
        /// </summary>
        public long receiveData { get; set; }
        /// <summary>
        /// 发送数据
        /// </summary>
        public long sendData { get; set; }
        /// <summary>
        /// 插件名称
        /// </summary>
        public string plugin { get; set; }
        /// <summary>
        /// 中间件标识，只有超级客户端才有值
        /// </summary>
        public string ServerIdentify { get; set; }

    }
}
