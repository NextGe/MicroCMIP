using EFWCoreLib.CoreFrame.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EFWCoreLib.CoreFrame.ProcessManage
{
    public class ProcessWatcher
    {
        //字段
        private static List<ProcessObject> _processObject;
        private static object _lockerForLog = new object();
        private static string _logPath = string.Empty;
        private static List<Thread> threadList;

        private static List<ProcessObject> getprocessObject()
        {
            List<ProcessObject> processlist = new List<ProcessObject>();

            if (efwplusHttpManager.isHttp)
            {
                ProcessObject po = new ProcessObject();
                po.processAddress = efwplusHttpManager.baseExe;
                po.processStart = efwplusHttpManager.StartHttp;

                processlist.Add(po);
            }

            if (efwplusBaseManager.Iswcfservice)
            {
                ProcessObject po = new ProcessObject();
                po.processAddress = efwplusBaseManager.baseExe;
                po.processStart = efwplusBaseManager.StartBase;

                processlist.Add(po);
            }

            if (efwplusRouteManager.Isrouter)
            {
                ProcessObject po = new ProcessObject();
                po.processAddress = efwplusRouteManager.routeExe;
                po.processStart = efwplusRouteManager.StartRoute;

                processlist.Add(po);
            }

            if (MongodbManager.Ismongodb)
            {
                ProcessObject po = new ProcessObject();
                po.processAddress = MongodbManager.mongodExe;
                po.processStart = MongodbManager.StartDB;

                processlist.Add(po);
            }

            if (NginxManager.Isnginx)
            {
                ProcessObject po = new ProcessObject();
                po.processAddress = NginxManager.nginxExe;
                po.processStart = NginxManager.StartWeb;

                processlist.Add(po);
            }

            return processlist;
        }


        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        public static void OnStart()
        {
            try
            {
                //读取监控进程全路径
                _processObject = getprocessObject();

                //创建日志目录
                _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "efwplusWatcherLog");
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }
            }
            catch (Exception ex)
            {
                ProcessWatcher.SaveLog("Watcher()初始化出错！错误描述为：" + ex.Message.ToString());
            }

            try
            {
                threadList = new List<Thread>();
                //Thread.Sleep(50000);
                UsDelay(5 * 1000);//5s
                StartWatch();
                //StartListen();
            }
            catch (Exception ex)
            {
                SaveLog("OnStart() 出错，错误描述：" + ex.Message.ToString());
            }
        }

        static System.Timers.Timer timer;
        //
        static void StartListen()
        {
            timer = new System.Timers.Timer();
            timer.Interval = 5000;//10s
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer.Enabled = false;
                StartWatch();
                //timer.Enabled = true;
            }
            catch
            {
                //timer.Enabled = true;
            }
        }


        /// <summary>
        /// 停止服务
        /// </summary>
        public static void OnStop()
        {
            try
            {
                if (threadList != null)
                {
                    foreach(var t in threadList)
                    {
                        t.Abort();
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLog("OnStop 出错，错误描述：" + ex.Message.ToString());
            }
        }


        /// <summary>
        /// 开始监控
        /// </summary>
        private static void StartWatch()
        {
            if (_processObject != null)
            {
                if (_processObject.Count > 0)
                {
                    foreach (var p in _processObject)
                    {
                        if (File.Exists(p.processAddress.Trim()))
                        {
                            ScanProcessList(p.processAddress.Trim(), p.processStart);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 扫描进程列表，判断进程对应的全路径是否与指定路径一致
        /// 如果一致，说明进程已启动
        /// 如果不一致，说明进程尚未启动
        /// </summary>
        /// <param name="strAddress"></param>
        private static void ScanProcessList(string address,Func<Process> start)
        {
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                //System、Idle进程会拒绝访问其全路径
                if (p.ProcessName != "System" && p.ProcessName != "Idle")
                {
                    try
                    {
                        if (FormatPath(address) == FormatPath(p.MainModule.FileName.ToString()))
                        {
                            //进程已启动
                            WatchProcess(p, start);
                            return;
                        }
                    }
                    catch
                    {
                        //拒绝访问进程的全路径
                        SaveLog("进程(" + p.Id.ToString() + ")(" + p.ProcessName.ToString() + ")拒绝访问全路径！");
                    }
                }
            }

            //进程尚未启动
            //Process process = new Process();
            //process.StartInfo.WorkingDirectory = new FileInfo(address).DirectoryName;
            //process.StartInfo.FileName = address;
            //process.StartInfo.UseShellExecute = false;
            //process.StartInfo.CreateNoWindow = true;
            //process.Start();

            

            WatchProcess(start(), start);
        }

        private static void UsDelay(int us)
        {
            long duetime = -10 * us;
            int hWaitTimer = WindowsAPI.CreateWaitableTimer(WindowsAPI.NULL, true, WindowsAPI.NULL);
            WindowsAPI.SetWaitableTimer(hWaitTimer, ref duetime, 0, WindowsAPI.NULL, WindowsAPI.NULL, false);
            while (WindowsAPI.MsgWaitForMultipleObjects(1, ref hWaitTimer, false, Timeout.Infinite, WindowsAPI.QS_TIMER)) ;
            WindowsAPI.CloseHandle(hWaitTimer);
        }

        /// <summary>
        /// 监听进程
        /// </summary>
        /// <param name="p"></param>
        /// <param name="address"></param>
        private static void WatchProcess(Process process,Func<Process> start)
        {
            ProcessRestart objProcessRestart = new ProcessRestart(process, start);
            Thread thread = new Thread(new ThreadStart(objProcessRestart.RestartProcess));
            thread.Start();

            threadList.Add(thread);
        }


        /// <summary>
        /// 格式化路径
        /// 去除前后空格
        /// 去除最后的"\"
        /// 字母全部转化为小写
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string FormatPath(string path)
        {
            return path.ToLower().Trim().TrimEnd('\\');
        }


        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="content"></param>
        public static void SaveLog(string content)
        {
            try
            {
                lock (_lockerForLog)
                {
                    FileStream fs;
                    fs = new FileStream(Path.Combine(_logPath, DateTime.Now.ToString("yyyyMMdd") + ".log"), FileMode.OpenOrCreate);
                    StreamWriter streamWriter = new StreamWriter(fs);
                    streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    streamWriter.WriteLine("[" + DateTime.Now.ToString() + "]：" + content);
                    streamWriter.Flush();
                    streamWriter.Close();
                    fs.Close();
                }
            }
            catch
            {
            }
        }

    }

    public class ProcessObject
    {
        public string processAddress { get; set; }
        public Func<Process> processStart { get; set; }
    }

    public class ProcessRestart
    {
        //字段
        private Process _process;
        private Func<Process> _start;


        /// <summary>
        /// 构造函数
        /// </summary>
        public ProcessRestart()
        { }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="process"></param>
        /// <param name="address"></param>
        public ProcessRestart(Process process, Func<Process> start)
        {
            this._process = process;
            this._start = start;
        }


        /// <summary>
        /// 重启进程
        /// </summary>
        public void RestartProcess()
        {
            try
            {
                while (true)
                {
                    this._process.WaitForExit();
                    //this._process.Close();    //释放已退出进程的句柄
                    //this._process.StartInfo.FileName = this._address;
                    //this._process.Start();
                    this._process = this._start();

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                //ProcessWatcher objProcessWatcher = new ProcessWatcher();
                ProcessWatcher.SaveLog("RestartProcess() 出错，监控程序已取消对进程("
                    + this._process.Id.ToString() + ")(" + this._process.ProcessName.ToString()
                    + ")的监控，错误描述为：" + ex.Message.ToString());
            }
        }


    }
}
