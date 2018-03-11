using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DRM.PropBag.XMLModel
{
    [XmlRoot("model")]
    public class XMLPropModel
    {
        DeriveFromClassModeEnum _deriveFromClassMode;
        [XmlAttribute(AttributeName = "derive-from-class-mode")]
        public DeriveFromClassModeEnum DeriveFromClassMode { get; set; }

        [XmlAttribute(AttributeName = "name-of-class-to-wrap")]
        public string NameOfClassToWrap { get; set; }

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
        public List<XMLPropItemModel> Props { get; set; }

        public XMLPropModel() : this("UndefinedClassName", "UndefinedNameSpace") { }


        public XMLPropModel
            (
            string className,
            string namespaceName,
            DeriveFromClassModeEnum deriveFromClassMode = DeriveFromClassModeEnum.Custom,
            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered,
            bool deferMethodRefResolution = true,
            bool requireExplicitInitialValue = true
            )
        {
            DeriveFromClassMode = deriveFromClassMode;
            ClassName = className;
            Namespace = namespaceName;
            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
            RequireExplicitInitialValue = requireExplicitInitialValue;
            Namespaces = new List<string>();
            Props = new List<XMLPropItemModel>();
        }
    }

}
