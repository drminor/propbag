using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel
{

    //public enum PropComparerType
    //{
    //    typed,
    //    @object,
    //    reference
    //}

    public class PropComparerField
    {
        //[XmlAttribute("comparer-type")]
        //public PropComparerType ComparerType { get; set; }

        [XmlText]
        public string Comparer { get; set; }

        [XmlAttribute("use-reference-equality")]
        public bool UseRefEquality { get; set; }

        public PropComparerField() : this(null) { }

        public PropComparerField(string comparer, bool useRefEquality = false)
        {
            Comparer = comparer;
            UseRefEquality = useRefEquality;
        }
    }
}
