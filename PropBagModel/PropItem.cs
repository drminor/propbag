using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBagModel
{

    public enum PropStorageType
    {
        @internal,
        external,
        none
    }

    public class PropItem
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("initial-value", IsNullable=true)]
        public string InitialValue { get; set; }

        [XmlElement("comparer", IsNullable = true)]
        public PropComparerField ComparerField { get; set; }

        //[XmlElement("comparer", IsNullable = true)]
        //public string Comparer { get; set; }

        [XmlElement("do-when-changed", IsNullable = true)]
        public PropDoWhenChanged DoWhenChangedField { get; set; }

        //[XmlAttribute(AttributeName = "use-reference-equality")]
        //public bool UseReferenceEquality { get; set; }

        [XmlAttribute(AttributeName = "storage-type")]
        public PropStorageType StorageType { get; set; }


        public PropItem() : this(null, null) { }


        public PropItem(string name, string type, PropStorageType storageType = PropStorageType.@internal,
            PropDoWhenChanged doWhenChanged = null, PropComparerField comparer = null)
        {
            Name = name;
            Type = type;
            //UseReferenceEquality = useRefEquality;
            StorageType = storageType;
            ComparerField = comparer;
            DoWhenChangedField = doWhenChanged;

        }
    }
}
