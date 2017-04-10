using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace efwplusHttp
{
    public class MyHttpServer
    {
        public static Action<string> ShowMsg;
        public static void Listen()
        {
            HttpListener httpListener = new HttpListener();

            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListener.Prefixes.Add(ConfigurationSettings.AppSettings["baseUrl"]);
            httpListener.Start();

            string upgradeList= ConfigurationSettings.AppSettings["upgradeList"];
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;

            new Thread(new ThreadStart(delegate
            {
                while (true)
                {
                    HttpListenerContext httpListenerContext = httpListener.GetContext();
                    ShowMsg("执行请求："+ httpListenerContext.Request.RawUrl);
                    if (httpListenerContext.Request.RawUrl == "/")
                    {
                        httpListenerContext.Response.StatusCode = (int)HttpStatusCode.OK;// 200;
                        using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                        {
                            writer.WriteLine("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/><title>升级包下载</title></head><body>");
                            writer.WriteLine("<div style=\"color:black;text-align:center;font-size:18px\"><p>升级包下载</p></div>");
                            writer.WriteLine("<hr>");
                            string[] upgrades = upgradeList.Split(';');
                            foreach (string u in upgrades)
                            {
                                string filepath = rootPath+ u.Split('|')[1];
                                string[] files= Directory.GetFiles(filepath);
                                string html="";
                                foreach (string f in files)
                                {
                                    FileInfo file = new FileInfo(f);
                                    html += "<a href='" + f.Replace(rootPath, "") + "' >[" + file.Name + "]</a>";
                                }
                                writer.WriteLine("<div style=\"color:black;text-align:left;font-size:10px\"><p>" + u.Split('|')[0] + "："+html+"</p></div>");
                            }
                            writer.WriteLine("</body></html>");
                        }
                    }
                    else
                    {
                        httpListenerContext.Response.StatusCode = (int)HttpStatusCode.OK;// 200;
                        using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                        {
                            try
                            {
                                string filepath = rootPath + httpListenerContext.Request.RawUrl;
                                if (File.Exists(filepath))
                                {
                                    FileStream fs = File.OpenRead(filepath); //待下载的文件
                                    CopyStream(fs, writer.BaseStream);
                                    fs.Flush();
                                    fs.Close();
                                }
                                else
                                {
                                    ShowMsg("异常：找不到此文件" + httpListenerContext.Request.RawUrl);
                                    httpListenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;// 200;
                                }
                            }
                            catch (Exception e)
                            {
                                ShowMsg("异常：下载文件出错！\n\r" + httpListenerContext.Request.RawUrl + "\n\r" + e.Message);
                                httpListenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;// 200;
                            }
                        }
                    }

                    ShowMsg("请求完成：" + httpListenerContext.Request.RawUrl);
                }
            })).Start();
        }


        private static void CopyStream(Stream orgStream, Stream desStream)
        {
            byte[] buffer = new byte[1024];

            int read = 0;
            while ((read = orgStream.Read(buffer, 0, 1024)) > 0)
            {
                desStream.Write(buffer, 0, read);

                //System.Threading.Thread.Sleep(1000); //模拟慢速设备
            }
        }
    }
}
