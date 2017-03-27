using EFWCoreLib.WcfFrame.DataSerialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace EFWCoreLib.WcfFrame.WcfHandler
{
    /// <summary>
    /// 文件传输服务
    /// </summary>
    [ServiceKnownType(typeof(DBNull))]
    [ServiceContract(Namespace = "http://www.efwplus.cn/", Name = "FileService", SessionMode = SessionMode.Allowed)]
    public interface IFileHandler
    {
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="filestream">文件信息</param>
        /// <returns>返回上传结果</returns>
        [OperationContract]
        UpFileResult UpLoadFile(UpFile filestream);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="downfile">文件信息</param>
        /// <returns>返回下载结果</returns>
        [OperationContract]
        DownFileResult DownLoadFile(DownFile downfile);

        ///// <summary>
        ///// 下载转发
        ///// </summary>
        ///// <param name="result"></param>
        ///// <returns></returns>
        //[OperationContract]
        //DownFileResult DownLoadForward(DownFileResult result);

        ///// <summary>
        ///// 上传转发
        ///// </summary>
        ///// <param name="result"></param>
        ///// <returns></returns>
        //[OperationContract]
        //UpFileResult UpLoadForward(UpFileResult result);
    }
}
