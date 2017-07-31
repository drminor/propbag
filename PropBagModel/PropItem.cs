using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBagModel
{

    public class PropItem
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("initial-value", IsNullable=true)]
        public string InitialValue { get; set; }


        [XmlAttribute(AttributeName = "use-reference-equality")]
        public bool UseReferenceEquality { get; set; }

        public PropItem()
        {
            UseReferenceEquality = false;
        }

        public PropItem(string name, string type, bool useRefEquality = false)
        {
            Name = name;
            Type = type;
            UseReferenceEquality = useRefEquality;
        }
    }
}
