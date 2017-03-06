using System;
using System.Collections.Generic;
using System.Text;

namespace EFWCoreLib.CoreFrame.Common
{
    /// <summary>
    /// TimingTaskType ��ʱ��������� 
    /// </summary>
    [EnumDescription("��ʱ���������")]
    public enum TimingTaskType
    {
        [EnumDescription("ÿСʱһ��")]
        PerHour = 0,
        [EnumDescription("ÿ��һ��")]
        PerDay = 1,
        [EnumDescription("ÿ��һ��")]
        PerWeek = 2,
        [EnumDescription("ÿ��һ��")]
        PerMonth = 3
    }
}
