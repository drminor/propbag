using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBagModel
{

    public enum PropComparerType
    {
        typed,
        @object,
        reference
    }

    public class PropComparerField
    {
        [XmlAttribute("comparer-type")]
        public PropComparerType ComparerType { get; set; }

        [XmlText]
        public string Comparer { get; set; }

        public PropComparerField() : this(null) { }

        public PropComparerField(string comparer, PropComparerType cType = PropComparerType.typed)
        {
            ComparerType = cType;
            Comparer = comparer;
        }
    }
}
