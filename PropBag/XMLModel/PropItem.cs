using DRM.TypeSafePropertyBag;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.XMLModel
{

    //public enum PropStorageType
    //{
    //    @internal,
    //    external,
    //    none
    //}

    public class PropItem
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("initial-value")]
        public PropInitialValueField InitalValueField { get; set;}

        [XmlElement("comparer")]
        public PropComparerField ComparerField { get; set; }

        [XmlElement("do-when-changed")]
        public PropDoWhenChanged DoWhenChangedField { get; set; }

        [XmlAttribute(AttributeName = "caller-provides-storage")]
        public PropStorageStrategyEnum StorageStrategy { get; set; }

        [XmlIgnore]
        public bool TypeIsSolid { get; set; }

        [XmlIgnore]
        public string ExtraInfo { get; set; }

        public PropItem() : this(null, null) { }

        public PropItem(string type, string name,
            string extraInfo = null, PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal,
            bool typeIsSolid = true,
            PropDoWhenChanged doWhenChanged = null, PropComparerField comparer = null)
        {
            Type = type;
            Name = name;
            ExtraInfo = extraInfo;
            StorageStrategy = storageStrategy;
            TypeIsSolid = typeIsSolid;
            ComparerField = comparer;
            DoWhenChangedField = doWhenChanged;

        }
    }
}
