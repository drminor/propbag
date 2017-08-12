using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel
{
    public class PropComparerField : NotifyPropertyChangedBase, IEquatable<PropComparerField>
    {

        string c;
        bool ure;

        [XmlText]
        public string Comparer { get { return c; } set { SetIfDifferent<string>(ref c, value); } }

        [XmlAttribute("use-reference-equality")]
        public bool UseRefEquality { get { return ure; } set { SetIfDifferent<bool>(ref ure, value); } }

        public PropComparerField() : this(null) { }

        public PropComparerField(string comparer, bool useRefEquality = false)
        {
            Comparer = comparer;
            UseRefEquality = useRefEquality;
        }

        public bool Equals(PropComparerField other)
        {
            if (other == null) return false;

            if (other.Comparer == Comparer && other.UseRefEquality == UseRefEquality) return true;

            return false;
        }
    }
}
