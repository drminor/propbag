using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBagModel
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
        public PropIniialValueField InitalValueField { get; set;}

        [XmlElement("comparer")]
        public PropComparerField ComparerField { get; set; }

        [XmlElement("do-when-changed")]
        public PropDoWhenChanged DoWhenChangedField { get; set; }

        [XmlAttribute(AttributeName = "caller-provides-storage")]
        public bool HasStore { get; set; }

        public PropItem() : this(null, null) { }

        public PropItem(string name, string type, bool hasStore = true,
            PropDoWhenChanged doWhenChanged = null, PropComparerField comparer = null)
        {
            Name = name;
            Type = type;
            HasStore = hasStore;
            ComparerField = comparer;
            DoWhenChangedField = doWhenChanged;

        }
    }
}
