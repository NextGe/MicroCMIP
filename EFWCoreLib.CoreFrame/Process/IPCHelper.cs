using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace EFWCoreLib.CoreFrame.ProcessManage
{
    public enum IPCType
    {
        efwplusBase,efwplusRoute,efwplusWebAPI,efwplusServer
    }
    public class IPCName
    {
        public static uint mapLength=10240;            //共享内存长
        public static string GetWriteMapName(IPCType type)
        {
            switch (type)
            {
                case IPCType.efwplusBase:
                    return "WriteMap_efwplusBase";
                case IPCType.efwplusRoute:
                    return "WriteMap_efwplusRoute";
                case IPCType.efwplusWebAPI:
                    return "WriteMap_efwplusWebAPI";
                case IPCType.efwplusServer:
                    return "WriteMap_efwplusServer";
                //case IPCType.efwplusServerCmd:
                //    return "WriteMap_efwplusServerCmd";
            }
            return "WriteMap";
        }

        public static string GetReadMapName(IPCType type)
        {
            switch (type)
            {
                case IPCType.efwplusBase:
                    return "ReadMap_efwplusBase";
                case IPCType.efwplusRoute:
                    return "ReadMap_efwplusRoute";
                case IPCType.efwplusWebAPI:
                    return "ReadMap_efwplusWebAPI";
                case IPCType.efwplusServer:
                    return "ReadMap_efwplusServer";
                //case IPCType.efwplusServerCmd:
                //    return "ReadMap_efwplusServerCmd";
            }
            return "ReadMap";
        }

        public static string GetshareMemoryName(IPCType type)
        {
            switch (type)
            {
                case IPCType.efwplusBase:
                    return "shareMemory_efwplusBase";
                case IPCType.efwplusRoute:
                    return "shareMemory_efwplusRoute";
                case IPCType.efwplusWebAPI:
                    return "shareMemory_efwplusWebAPI";
                case IPCType.efwplusServer:
                    return "shareMemory_efwplusServer";
                //case IPCType.efwplusServerCmd:
                //    return "shareMemory_efwplusServerCmd";
            }
            return "shareMemory";
        }
        //得到共享地址
        public static uint GetshareAddress(IPCType type)
        {
            switch (type)
            {
                case IPCType.efwplusBase:
                    return 0x0002;
                case IPCType.efwplusWebAPI:
                    return 0x1002;
                case IPCType.efwplusRoute:
                    return 0x2002;
            }
            return 0x0002;
        }

        public static string GetProcessName(IPCType type)
        {
            switch (type)
            {
                case IPCType.efwplusBase:
                    return "efwplusbase";
                case IPCType.efwplusRoute:
                    return "efwplusroute";
                case IPCType.efwplusWebAPI:
                    return "efwpluswebapi";
                case IPCType.efwplusServer:
                    return "efwplusserver";
            }
            return "";
        }
    }
    /// <summary>
    /// 进程间通信，读取数据
    /// </summary>
    public class IPCReceiveHelper
    {
        const int INVALID_HANDLE_VALUE = -1;
        const int PAGE_READWRITE = 0x04;

        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //共享内存
        [DllImport("Kernel32.dll", EntryPoint = "CreateFileMapping")]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, //HANDLE hFile,
         UInt32 lpAttributes,//LPSECURITY_ATTRIBUTES lpAttributes,  //0
         UInt32 flProtect,//DWORD flProtect
         UInt32 dwMaximumSizeHigh,//DWORD dwMaximumSizeHigh,
         UInt32 dwMaximumSizeLow,//DWORD dwMaximumSizeLow,
         string lpName//LPCTSTR lpName
         );

        [DllImport("Kernel32.dll", EntryPoint = "OpenFileMapping")]
        private static extern IntPtr OpenFileMapping(
         UInt32 dwDesiredAccess,//DWORD dwDesiredAccess,
         int bInheritHandle,//BOOL bInheritHandle,
         string lpName//LPCTSTR lpName
         );

        const int FILE_MAP_ALL_ACCESS = 0x0002;
        const int FILE_MAP_WRITE = 0x0002;

        [DllImport("Kernel32.dll", EntryPoint = "MapViewOfFile")]
        private static extern IntPtr MapViewOfFile(
         IntPtr hFileMappingObject,//HANDLE hFileMappingObject,
         UInt32 dwDesiredAccess,//DWORD dwDesiredAccess
         UInt32 dwFileOffsetHight,//DWORD dwFileOffsetHigh,
         UInt32 dwFileOffsetLow,//DWORD dwFileOffsetLow,
         UInt32 dwNumberOfBytesToMap//SIZE_T dwNumberOfBytesToMap
         );

        [DllImport("Kernel32.dll", EntryPoint = "UnmapViewOfFile")]
        private static extern int UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("Kernel32.dll", EntryPoint = "CloseHandle")]
        private static extern int CloseHandle(IntPtr hObject);

        private Semaphore m_Write;  //可写的信号
        private Semaphore m_Read;  //可读的信号
        private IntPtr handle;     //文件句柄
        private IntPtr addr;       //共享内存地址
        //uint mapLength;            //共享内存长

        //线程用来读取数据
        Thread threadRed;
        Action<string> dataAction;//数据委托
        ///<summary>
        /// 初始化共享内存数据 创建一个共享内存
        ///</summary>
        public void Init(Action<string> _dataAction, IPCType type, IPCType addrtype)
        {
            dataAction = _dataAction;

            m_Write = new Semaphore(1, 1, IPCName.GetWriteMapName(type));//开始的时候有一个可以写
            m_Read = new Semaphore(0, 1, IPCName.GetReadMapName(type));//没有数据可读
            //mapLength = 10240000;
            IntPtr hFile = new IntPtr(INVALID_HANDLE_VALUE);
            handle = CreateFileMapping(hFile, 0, PAGE_READWRITE, 0, IPCName.mapLength, IPCName.GetshareMemoryName(type));
            addr = MapViewOfFile(handle, IPCName.GetshareAddress(addrtype), 0, 0, 0);

            //handle = OpenFileMapping(0x0002, 0, "shareMemory");
            //addr = MapViewOfFile(handle, FILE_MAP_ALL_ACCESS, 0, 0, 0);

            threadRed = new Thread(new ThreadStart(ReceiveData));
            threadRed.Start();
        }

        /// <summary>
        /// 线程启动从共享内存中获取数据信息 
        /// </summary>
        private void ReceiveData()
        {
            //myDelegate myI = new myDelegate(changeTxt);
            while (true)
            {
                try
                {
                    //m_Write = Semaphore.OpenExisting("WriteMap");
                    //m_Read = Semaphore.OpenExisting("ReadMap");
                    //handle = OpenFileMapping(FILE_MAP_WRITE, 0, "shareMemory");

                    //读取共享内存中的数据：
                    //是否有数据写过来
                    m_Read.WaitOne();
                    //IntPtr m_Sender = MapViewOfFile(handle, FILE_MAP_ALL_ACCESS, 0, 0, 0);
                    byte[] byteStr = new byte[IPCName.mapLength];
                    byteCopy(byteStr, addr);
                    string str = Encoding.Default.GetString(byteStr, 0, byteStr.Length);
                    //调用数据处理方法 处理读取到的数据
                    m_Write.Release();

                    if(dataAction!=null)
                    {
                        dataAction(str.Substring(0, str.IndexOf('\0')));
                    }
                    Thread.Sleep(50);
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    continue;
                    //Thread.Sleep(0);
                }
            }

        }
        //不安全的代码在项目生成的选项中选中允许不安全代码
        static unsafe void byteCopy(byte[] dst, IntPtr src)
        {
            fixed (byte* pDst = dst)
            {
                byte* pdst = pDst;
                byte* psrc = (byte*)src;
                while ((*pdst++ = *psrc++) != '\0')
                    ;
            }

        }
    }

    /// <summary>
    /// 进程间通信，写入数据
    /// </summary>
    public class IPCWriteHelper
    {
        const int INVALID_HANDLE_VALUE = -1;
        const int PAGE_READWRITE = 0x04;
        //共享内存
        [DllImport("Kernel32.dll", EntryPoint = "CreateFileMapping")]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, //HANDLE hFile,
         UInt32 lpAttributes,//LPSECURITY_ATTRIBUTES lpAttributes,  //0
         UInt32 flProtect,//DWORD flProtect
         UInt32 dwMaximumSizeHigh,//DWORD dwMaximumSizeHigh,
         UInt32 dwMaximumSizeLow,//DWORD dwMaximumSizeLow,
         string lpName//LPCTSTR lpName
         );

        [DllImport("Kernel32.dll", EntryPoint = "OpenFileMapping")]
        private static extern IntPtr OpenFileMapping(
         UInt32 dwDesiredAccess,//DWORD dwDesiredAccess,
         int bInheritHandle,//BOOL bInheritHandle,
         string lpName//LPCTSTR lpName
         );

        const int FILE_MAP_ALL_ACCESS = 0x0002;
        const int FILE_MAP_WRITE = 0x0002;

        [DllImport("Kernel32.dll", EntryPoint = "MapViewOfFile")]
        private static extern IntPtr MapViewOfFile(
         IntPtr hFileMappingObject,//HANDLE hFileMappingObject,
         UInt32 dwDesiredAccess,//DWORD dwDesiredAccess
         UInt32 dwFileOffsetHight,//DWORD dwFileOffsetHigh,
         UInt32 dwFileOffsetLow,//DWORD dwFileOffsetLow,
         UInt32 dwNumberOfBytesToMap//SIZE_T dwNumberOfBytesToMap
         );

        [DllImport("Kernel32.dll", EntryPoint = "UnmapViewOfFile")]
        private static extern int UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("Kernel32.dll", EntryPoint = "CloseHandle")]
        private static extern int CloseHandle(IntPtr hObject);



        private Semaphore m_Write;  //可写的信号
        private Semaphore m_Read;  //可读的信号
        private IntPtr handle;     //文件句柄
        private IntPtr addr;       //共享内存地址
        //uint mapLength=10240000;            //共享内存长

        //Thread threadRed;

        public void WriteData(string data, IPCType type, IPCType addrtype)
        {
            try
            {
                m_Write = Semaphore.OpenExisting(IPCName.GetWriteMapName(type));
                m_Read = Semaphore.OpenExisting(IPCName.GetReadMapName(type));
                handle = OpenFileMapping(FILE_MAP_WRITE, 0, IPCName.GetshareMemoryName(type));
                addr = MapViewOfFile(handle, IPCName.GetshareAddress(addrtype), 0, 0, 0);

                m_Write.WaitOne();
                byte[] sendStr = Encoding.Default.GetBytes(data + '\0');
                //如果要是超长的话，应另外处理，最好是分配足够的内存
                if (sendStr.Length < IPCName.mapLength)
                    Copy(sendStr, addr);

                m_Read.Release();
            }
            catch
            {
                //throw new Exception("不存在系统信号量!");
            }
        }

        static unsafe void Copy(byte[] byteSrc, IntPtr dst)
        {
            fixed (byte* pSrc = byteSrc)
            {
                byte* pDst = (byte*)dst;
                byte* psrc = pSrc;
                for (int i = 0; i < byteSrc.Length; i++)
                {
                    *pDst = *psrc;
                    pDst++;
                    psrc++;
                }
            }
        }
    }

    /// <summary>
    /// 进程之间通信中间进程
    /// </summary>
    public class efwplusServerIPCManager
    {
        //IPCReceiveHelper ipcr;//接收其他进程的数据
        IPCWriteHelper ipcw_base;//往其他进程写入数据
        IPCWriteHelper ipcw_api;//往其他进程写入数据
        IPCWriteHelper ipcw_route;//往其他进程写入数据
        Func<string, Dictionary<string, string>, string> funcExecCmd;//委托执行命令
        Action<string> actionReceiveData;//委托收到数据
        Action<CmdObject> actionReturnData;//返回数据
        private Object syncObj = new Object();//定义一个静态对象用于线程部份代码块的锁定，用于lock操作

        public efwplusServerIPCManager(Func<string, Dictionary<string, string>, string> _funcExecCmd, Action<string> _actionReceiveData)
        {
            funcExecCmd = _funcExecCmd;
            actionReceiveData = _actionReceiveData;
            Action<string> action = ((string text) =>
            {
                if (CmdObject.AnalysisCmdText(text).typestr == "cmd")
                    ExecuteCmd(text);
                else if (CmdObject.AnalysisCmdText(text).typestr == "data")
                    ReceiveData(text);
                else
                    return;//无效的文本
            });


            ipcw_base = new IPCWriteHelper();
            ipcw_api = new IPCWriteHelper();
            ipcw_route = new IPCWriteHelper();
            IPCReceiveHelper ipcr_base = new IPCReceiveHelper();
            ipcr_base.Init(action, IPCType.efwplusServer, IPCType.efwplusBase);

            IPCReceiveHelper ipcr_api = new IPCReceiveHelper();
            ipcr_api.Init(action, IPCType.efwplusServer, IPCType.efwplusWebAPI);

            IPCReceiveHelper ipcr_route = new IPCReceiveHelper();
            ipcr_route.Init(action, IPCType.efwplusServer, IPCType.efwplusRoute);
        }
        /// <summary>
        /// 执行命令
        /// cmd#efwpluswebapi-efwplusbase#setmnodestate@name=111&state=1
        /// data#efwplusbase-efwpluswebapi#msgtext
        /// </summary>
        /// <param name="cmd"></param>
        void ExecuteCmd(string cmd)
        {
            try
            {
                string bprocess = CmdObject.AnalysisCmdText(cmd).pathstr_begin;//开始发送执行命令的进程名
                string eprocess = CmdObject.AnalysisCmdText(cmd).pathstr_end;//目标执行命令的进程名

                if (eprocess == "efwplusserver")
                {
                    //执行efwplusserver进程的命令
                    if (funcExecCmd != null)
                    {
                        string retval = funcExecCmd(CmdObject.AnalysisCmdText(cmd).methodstr, CmdObject.AnalysisCmdText(cmd).argdic);//执行命令 arg1方法名 arg2参数

                        string datatext = CmdObject.BuildDataText(CmdObject.AnalysisCmdText(cmd).pathstr_end, CmdObject.AnalysisCmdText(cmd).pathstr_begin, CmdObject.AnalysisCmdText(cmd).uniqueid, retval);
                        //返回结果写回调用命令的进程
                        if (bprocess == "efwplusbase")
                            ipcw_base.WriteData(datatext, IPCType.efwplusBase, IPCType.efwplusBase);
                        else if (bprocess == "efwpluswebapi")
                            ipcw_api.WriteData(datatext, IPCType.efwplusWebAPI, IPCType.efwplusWebAPI);
                        else if (bprocess == "efwplusroute")
                            ipcw_route.WriteData(datatext, IPCType.efwplusRoute, IPCType.efwplusRoute);
                    }
                }
                else if (eprocess == "efwplusbase")
                {
                    ipcw_base.WriteData(cmd, IPCType.efwplusBase, IPCType.efwplusBase);
                }
                else if (eprocess == "efwpluswebapi")
                {
                    ipcw_api.WriteData(cmd, IPCType.efwplusWebAPI, IPCType.efwplusWebAPI);
                }
                else if (eprocess == "efwplusroute")
                {
                    ipcw_route.WriteData(cmd, IPCType.efwplusRoute, IPCType.efwplusRoute);
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                throw new Exception("命令格式错误:" + e.Message);
            }
        }

        /// <summary>
        /// 收到数据
        /// </summary>
        /// <param name="data"></param>
        void ReceiveData(string data)
        {
            try
            {
                string eprocess = CmdObject.AnalysisCmdText(data).pathstr_end;//目标执行命令的进程名
                if (eprocess == "efwplusserver")//显示数据
                {
                    if (actionReceiveData != null)
                    {
                        actionReceiveData(CmdObject.AnalysisCmdText(data).retdata);
                    }
                    if (actionReturnData != null)
                    {
                        actionReturnData(CmdObject.AnalysisCmdText(data));
                    }
                }
                else if (eprocess == "efwplusbase")
                {
                    ipcw_base.WriteData(data, IPCType.efwplusBase, IPCType.efwplusBase);
                }
                else if (eprocess == "efwpluswebapi")
                {
                    ipcw_api.WriteData(data, IPCType.efwplusWebAPI, IPCType.efwplusWebAPI);
                }
                else if (eprocess == "efwplusroute")
                {
                    ipcw_route.WriteData(data, IPCType.efwplusRoute, IPCType.efwplusRoute);
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                throw new Exception("命令格式错误:" + e.Message);
            }
        }

        public void ShowMsg(string msg)
        {
            if (actionReceiveData != null)
            {
                //tring text = ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + msg);
                actionReceiveData(msg);
            }
        }

        public string CallCmd(string eprocess, string method, string arg)
        {
            lock (syncObj)
            {
                string cmdtext = "";
                bool IsCompleted = false;//是否完成
                string retData = "";
                actionReturnData = ((CmdObject cobj) =>
                {
                    if (cobj.uniqueid == CmdObject.AnalysisCmdText(cmdtext).uniqueid)
                    {
                        retData = cobj.retdata;
                        IsCompleted = true;
                    }
                });
                cmdtext = CmdObject.BuildCmdText("efwplusserver", eprocess, Guid.NewGuid().ToString(), method, arg);


                if (eprocess == "efwplusbase")
                {
                    ipcw_base.WriteData(cmdtext, IPCType.efwplusBase, IPCType.efwplusBase);
                }
                else if (eprocess == "efwpluswebapi")
                {
                    ipcw_api.WriteData(cmdtext, IPCType.efwplusWebAPI, IPCType.efwplusWebAPI);
                }
                else if (eprocess == "efwplusroute")
                {
                    ipcw_route.WriteData(cmdtext, IPCType.efwplusRoute, IPCType.efwplusRoute);
                }
                //超时计时器
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //是否超时
                bool isouttime = false;
                while (!IsCompleted)
                {
                    if (IsCompleted) break;
                    //如果还未获取连接判断是否超时5秒，如果超时抛异常
                    if (sw.Elapsed >= new TimeSpan(5 * 1000 * 10000))
                    {
                        isouttime = true;
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                if (isouttime)
                {
                    throw new Exception("命令执行超时");
                }

                return retData;
            }
        }
    }

    /// <summary>
    /// 普通进程通信
    /// </summary>
    public class NormalIPCManager
    {
        IPCType IPCType;
        IPCReceiveHelper ipcr;//接收其他进程的数据
        IPCWriteHelper ipcw;//往其他进程写入数据
        Func<string, Dictionary<string,string>, string> funcExecCmd;//委托执行命令
        Action<string> actionReceiveData;//委托收到数据
        Action<CmdObject> actionReturnData;//返回数据
        private Object syncObj = new Object();//定义一个静态对象用于线程部份代码块的锁定，用于lock操作
        public NormalIPCManager(IPCType _ipctype, Func<string, Dictionary<string,string>, string> _funcExecCmd, Action<string> _actionReceiveData)
        {
            IPCType = _ipctype;
            funcExecCmd = _funcExecCmd;
            actionReceiveData = _actionReceiveData;
            ipcw = new IPCWriteHelper();
            ipcr = new IPCReceiveHelper();

            Action<string> action = ((string text) =>
            {
                if (CmdObject.AnalysisCmdText(text).typestr == "cmd")
                    ExecuteCmd(text);
                else if (CmdObject.AnalysisCmdText(text).typestr == "data")
                    ReceiveData(text);
                else
                    return;//无效的文本
            });
            ipcr.Init(action, _ipctype, _ipctype);
        }

        /// <summary>
        /// 执行命令
        /// cmd#efwpluswebapi-efwplusbase#setmnodestate@name=111&state=1
        /// data#efwplusbase-efwpluswebapi#msgtext
        /// </summary>
        /// <param name="cmd"></param>
        void ExecuteCmd(string cmd)
        {
            try
            {
                //string bprocess = CmdObject.AnalysisCmdText(cmd).pathstr_begin;//开始发送执行命令的进程名
                string eprocess = CmdObject.AnalysisCmdText(cmd).pathstr_end;//目标执行命令的进程名
                string retval, datatext;
                switch (eprocess)
                {
                    case "efwplusbase":
                        retval = funcExecCmd(CmdObject.AnalysisCmdText(cmd).methodstr, CmdObject.AnalysisCmdText(cmd).argdic);//执行命令 arg1方法名 arg2参数
                        datatext = CmdObject.BuildDataText(CmdObject.AnalysisCmdText(cmd).pathstr_end, CmdObject.AnalysisCmdText(cmd).pathstr_begin, CmdObject.AnalysisCmdText(cmd).uniqueid, retval);
                        ipcw.WriteData(datatext, IPCType.efwplusServer, IPCType);
                        break;
                    case "efwpluswebapi":
                        retval = funcExecCmd(CmdObject.AnalysisCmdText(cmd).methodstr, CmdObject.AnalysisCmdText(cmd).argdic);//执行命令 arg1方法名 arg2参数
                        datatext = CmdObject.BuildDataText(CmdObject.AnalysisCmdText(cmd).pathstr_end, CmdObject.AnalysisCmdText(cmd).pathstr_begin, CmdObject.AnalysisCmdText(cmd).uniqueid, retval);
                        ipcw.WriteData(datatext, IPCType.efwplusServer, IPCType);
                        break;
                    case "efwplusroute":
                        retval = funcExecCmd(CmdObject.AnalysisCmdText(cmd).methodstr, CmdObject.AnalysisCmdText(cmd).argdic);//执行命令 arg1方法名 arg2参数
                        datatext = CmdObject.BuildDataText(CmdObject.AnalysisCmdText(cmd).pathstr_end, CmdObject.AnalysisCmdText(cmd).pathstr_begin, CmdObject.AnalysisCmdText(cmd).uniqueid, retval);
                        ipcw.WriteData(datatext, IPCType.efwplusServer, IPCType);
                        break;
                    default:
                        return;
                }
            }
            catch (Exception e)
            {
                throw new Exception("命令格式错误:" + e.Message);
            }
        }

        /// <summary>
        /// 收到数据
        /// </summary>
        /// <param name="data"></param>
        void ReceiveData(string data)
        {
            try
            {
                string eprocess = CmdObject.AnalysisCmdText(data).pathstr_end;//目标执行命令的进程名
                switch (eprocess)
                {
                    case "efwplusbase":
                    case "efwpluswebapi":
                    case "efwplusroute":
                        if (actionReceiveData != null)
                        {
                            actionReceiveData(CmdObject.AnalysisCmdText(data).retdata);
                        }
                        if (actionReturnData != null)
                        {
                            actionReturnData(CmdObject.AnalysisCmdText(data));
                        }
                        break;
                    default:
                        return;
                }
            }
            catch (Exception e)
            {
                throw new Exception("命令格式错误:" + e.Message);
            }
        }
        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="msg"></param>
        public void ShowMsg(string msg)
        {
            if (actionReceiveData != null)
            {
                actionReceiveData(msg);
            }
            string datatext = "";
            if (IPCType == IPCType.efwplusBase)
            {
                datatext = CmdObject.BuildDataText("efwplusbase", "efwplusserver",Guid.NewGuid().ToString(), msg);
                ipcw.WriteData(datatext, IPCType.efwplusServer, IPCType);
            }
            else if (IPCType == IPCType.efwplusRoute)
            {
                datatext = CmdObject.BuildDataText("efwplusroute", "efwplusserver", Guid.NewGuid().ToString(), msg);
                ipcw.WriteData(datatext, IPCType.efwplusServer, IPCType);
            }
            else if (IPCType == IPCType.efwplusWebAPI)
            {
                datatext = CmdObject.BuildDataText("efwpluswebapi", "efwplusserver", Guid.NewGuid().ToString(), msg);
                ipcw.WriteData(datatext, IPCType.efwplusServer, IPCType);
            }
            
        }
        public string CallCmd(string eprocess, string method, string arg)
        {

            if (IPCName.GetProcessName(IPCType) == eprocess)
            {
                string cmdtext = CmdObject.BuildCmdText(IPCName.GetProcessName(IPCType), eprocess, Guid.NewGuid().ToString(), method, arg);
                string retval = funcExecCmd(CmdObject.AnalysisCmdText(cmdtext).methodstr, CmdObject.AnalysisCmdText(cmdtext).argdic);//执行命令 arg1方法名 arg2参数
                //string datatext = CmdObject.BuildDataText(CmdObject.AnalysisCmdText(cmdtext).pathstr_end, CmdObject.AnalysisCmdText(cmdtext).pathstr_begin, CmdObject.AnalysisCmdText(cmdtext).uniqueid, retval);
                return retval;
            }

            lock (syncObj)
            {
                string cmdtext = "";
                bool IsCompleted = false;//是否完成
                string retData = "";
                actionReturnData = ((CmdObject cobj) =>
                {
                    if (cobj.uniqueid == CmdObject.AnalysisCmdText(cmdtext).uniqueid)
                    {
                        retData = cobj.retdata;
                        IsCompleted = true;
                    }
                });


                if (IPCType == IPCType.efwplusBase)
                {
                    cmdtext = CmdObject.BuildCmdText("efwplusbase", eprocess, Guid.NewGuid().ToString(), method, arg);
                    ipcw.WriteData(cmdtext, IPCType.efwplusServer, IPCType);
                }
                else if (IPCType == IPCType.efwplusRoute)
                {
                    cmdtext = CmdObject.BuildCmdText("efwplusroute", eprocess, Guid.NewGuid().ToString(), method, arg);
                    ipcw.WriteData(cmdtext, IPCType.efwplusServer, IPCType);
                }
                else if (IPCType == IPCType.efwplusWebAPI)
                {
                    cmdtext = CmdObject.BuildCmdText("efwpluswebapi", eprocess, Guid.NewGuid().ToString(), method, arg);
                    ipcw.WriteData(cmdtext, IPCType.efwplusServer, IPCType);
                }

                //超时计时器
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //是否超时
                bool isouttime = false;
                while (!IsCompleted)
                {
                    if (IsCompleted) break;
                    //如果还未获取连接判断是否超时5秒，如果超时抛异常
                    if (sw.Elapsed >= new TimeSpan(5 * 1000 * 10000))
                    {
                        isouttime = true;
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                if (isouttime)
                {
                    throw new Exception("命令执行超时");
                }
                return retData;
            }
        }
    }
    /// <summary>
    /// 命令对象
    /// </summary>
    public class CmdObject
    {
        //唯一标识
        public string uniqueid { get; set; }
        //命令类型
        public string typestr { get; set; }
        //命令路径开始进程
        public string pathstr_begin { get; set; }
        //命令路径结束进程
        public string pathstr_end { get; set; }
        //方法
        public string methodstr { get; set; }
        //参数
        public string argstr { get; set; }
        public Dictionary<string, string> argdic { get; set; }
        //返回数据
        public string retdata { get; set; }
        /// <summary>
        /// 解析命令
        /// </summary>
        /// <param name="cmdtext"></param>
        /// <returns></returns>
        public static CmdObject AnalysisCmdText(string cmdtext)
        {
            cmdtext = cmdtext.ToLower();
            CmdObject co = new CmdObject();
            int _c1 = cmdtext.IndexOf('#') + 1;
            int _c2 = cmdtext.IndexOf('#', _c1) + 1;
            int _c3 = cmdtext.IndexOf('#', _c2) + 1;
            co.typestr = cmdtext.Substring(0, _c1 - 1);
            string pathstr = cmdtext.Substring(_c1, _c2 - _c1 - 1);
            co.pathstr_begin = pathstr.Split('-')[0];
            co.pathstr_end = pathstr.Split('-')[1];
            co.uniqueid = cmdtext.Substring(_c2, _c3 - _c2 - 1);
            if (co.typestr == "cmd")
            {
                string cmdstr = cmdtext.Substring(_c3, cmdtext.Length - (_c3));

                int _a1 = cmdstr.IndexOf('@') + 1;
                co.methodstr = cmdstr.Substring(0, _a1 - 1);
                co.argstr = cmdstr.Substring(_a1, cmdstr.Length - _a1);
                if (co.argstr.Trim() != "")
                {
                    co.argdic = new Dictionary<string, string>();
                    string[] args = co.argstr.Split('&');
                    foreach(string a in args)
                    {
                        co.argdic.Add(a.Split('=')[0], a.Split('=')[1]);
                    }
                }
            }
            else if (co.typestr == "data")
            {
                co.retdata = cmdtext.Substring(_c3, cmdtext.Length - (_c3));
            }

            return co;
        }
        /// <summary>
        /// 构建命令文本
        /// </summary>
        /// <returns></returns>
        public static string BuildCmdText(string bprocess, string eprocess, string uqid, string method, string arg)
        {
            return "cmd#" + bprocess + "-" + eprocess + "#" + uqid + "#" + method + "@" + arg;
        }

        /// <summary>
        /// 构建数据文本
        /// </summary>
        /// <returns></returns>
        public static string BuildDataText(string bprocess, string eprocess, string uqid, string data)
        {
            return "data#" + bprocess + "-" + eprocess + "#" + uqid + "#" + data;
        }
    }
}