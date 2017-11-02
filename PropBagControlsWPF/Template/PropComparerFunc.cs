using System;
using System.ComponentModel;

namespace DRM.PropBag.ControlsWPF
{

    [TypeConverter(typeof(ComparerFuncTypeConverter))]
    public class PropComparerFunc : IEquatable<PropComparerFunc>
    {
        public Delegate Comparer { get; set; }

        public PropComparerFunc() : this(null) { }

        public PropComparerFunc(Delegate comparer)
        {
            Comparer = comparer;
        }

        public bool Equals(PropComparerFunc other)
        {
            if (other == null) return false;
            if (other.Comparer == Comparer) return true;
            return false;
        }
    }
}
