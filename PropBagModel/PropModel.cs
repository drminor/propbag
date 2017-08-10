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
        [XmlAttribute(AttributeName = "derive-from-pub-prop-bag")]
        public bool DeriveFromPubPropBag { get; set; }

        [XmlAttribute (AttributeName="class-name")]
        public string ClassName { get; set; }

        [XmlAttribute(AttributeName = "output-namespace")]
        public string Namespace { get; set; }

        [XmlAttribute(AttributeName = "type-safety-mode")]
        public PropBagTypeSafetyMode TypeSafetyMode { get; set; }

        [XmlAttribute(AttributeName = "defer-method-ref-resolution")]
        public bool DeferMethodRefResolution { get; set; }

        [XmlAttribute(AttributeName = "require-explicit-initial-value")]
        public bool RequireExplicitInitialValue { get; set; }

        [XmlArray("namespaces")]
        [XmlArrayItem("namespace")]
        public List<string> Namespaces { get; set; }

        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public List<PropItem> Props { get; set; }

        public PropModel() : this("UndefinedClassName", "UndefinedNameSpace") { }


        public PropModel(string className, string namespaceName,
            bool deriveFromPubPropBag = false,
            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered,
            bool deferMethodRefResolution = true,
            bool requireExplicitInitialValue = true)
        {
            DeriveFromPubPropBag = deriveFromPubPropBag;
            ClassName = className;
            Namespace = namespaceName;
            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
            RequireExplicitInitialValue = requireExplicitInitialValue;
            Namespaces = new List<string>();
            Props = new List<PropItem>();
        }
    }

}
