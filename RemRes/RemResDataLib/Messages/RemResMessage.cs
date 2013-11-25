using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemResDataLib.Messages
{
    [Serializable]
    [XmlInclude(typeof(AddWatchRule))]
    [XmlInclude(typeof(ClearWatchRules))]
    [XmlInclude(typeof(DeleteWatchRule))]
    [XmlInclude(typeof(GetProcessList))]
    [XmlInclude(typeof(GetProcessListResult))]
    [XmlInclude(typeof(GetSetting))]
    [XmlInclude(typeof(GetSettingResult))]
    [XmlInclude(typeof(GetWatchData))]
    [XmlInclude(typeof(GetWatchDataResult))]
    [XmlInclude(typeof(GetWatchRuleResult))]
    [XmlInclude(typeof(GetWatchRules))]
    [XmlInclude(typeof(KeepAliveRequest))]
    [XmlInclude(typeof(KeepAliveResponse))]
    [XmlInclude(typeof(Notification))]
    [XmlInclude(typeof(NotifyMe))]
    [XmlInclude(typeof(OperationStatus))]
    [XmlInclude(typeof(SetSetting))]
    public abstract class RemResMessage
    {
    }
}
