using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBagModel
{

    public class PropIniialValueField
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

        public PropIniialValueField() : this(null) { }

        public PropIniialValueField(string initialValue, bool setToDefault = false,
            bool setToUndefined = false, bool setToNull = false,
            bool setToEmptyString = false)
        {
            InitialValue = initialValue;
            SetToUndefined = setToUndefined;
            SetToDefault = setToDefault;
            SetToNull = setToNull;
            SetToEmptyString = setToEmptyString;
        }
    }
}