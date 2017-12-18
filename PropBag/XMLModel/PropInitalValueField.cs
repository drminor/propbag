using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.XMLModel
{
    // TODO: Add Create New Flag.
    public class PropInitialValueField
    {
        [XmlText]
        public string InitialValue { get; set; }

        [XmlAttribute("use-undefined")]
        public bool SetToUndefined { get; set; }

        [XmlAttribute("use-default")]
        public bool SetToDefault { get; set; }

        [XmlAttribute("use-null")]
        public bool SetToNull { get; set; }

        [XmlAttribute("use-empty-string")]
        public bool SetToEmptyString { get; set; }

        [XmlAttribute("create-new")]
        public bool CreateNew { get; set; }

        [XmlAttribute("propbag-resource-key")]
        public string PropBagResourceKey { get; set; }

        public PropInitialValueField() : this(null) { }

        public PropInitialValueField(string initialValue, bool setToDefault = false,
            bool setToUndefined = false, bool setToNull = false,
            bool setToEmptyString = false, bool createNew = false, string propBagResourceKey = null)
        {
            InitialValue = initialValue;
            SetToUndefined = setToUndefined;
            SetToDefault = setToDefault;
            SetToNull = setToNull;
            SetToEmptyString = setToEmptyString;
            CreateNew = createNew;
            PropBagResourceKey = propBagResourceKey;
        }
    }
}