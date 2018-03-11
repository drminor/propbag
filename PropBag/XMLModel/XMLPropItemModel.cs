using DRM.TypeSafePropertyBag;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.XMLModel
{
    public class XMLPropItemModel
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("initial-value")]
        public PropInitialValueField InitialValueField { get; set;}

        [XmlElement("comparer")]
        public PropComparerField ComparerField { get; set; }

        [XmlElement("do-when-changed")]
        public PropDoWhenChanged DoWhenChangedField { get; set; }

        [XmlAttribute(AttributeName = "storage-strategy")]
        public PropStorageStrategyEnum StorageStrategy { get; set; }

        [XmlIgnore]
        public bool TypeIsSolid { get; set; }

        [XmlIgnore]
        public string ExtraInfo { get; set; }

        public XMLPropItemModel() : this(null, null) { }

        public XMLPropItemModel(string type, string name,
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
