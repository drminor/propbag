using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

using DRM.PropBag;

namespace DRM.PropBagModel
{
    [XmlRoot("model")]
    public class PropModel
    {
        [XmlAttribute (AttributeName="class-name")]
        public string ClassName { get; set; }

        [XmlAttribute(AttributeName = "namespace")]
        public string Namespace { get; set; }

        [XmlAttribute(AttributeName = "type-safety-mode")]
        public PropBagTypeSafetyMode TypeSafetyMode { get; set; }

        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public PropItem[] Props { get; set; }


        public PropModel()
        {
            TypeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered;
        }

        public PropModel(string className, string namespaceName, PropBagTypeSafetyMode typeSafetyMode)
        {
            ClassName = className;
            Namespace = namespaceName;
            TypeSafetyMode = typeSafetyMode;
        }

        public string SafetyModeString
        {
            get
            {
                return TypeSafetyMode.ToString();
            }
        }
    }
}
