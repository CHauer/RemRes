using System;
using System.Xml.Serialization;

namespace RemResDataLib.BaseTypes
{
    [Serializable]
    public enum WatchFieldType
    {
        [XmlEnum]
        String,
        
        [XmlEnum]
        Integer,

        [XmlEnum]
        Double,

        [XmlEnum]
        DateTime

    }
}