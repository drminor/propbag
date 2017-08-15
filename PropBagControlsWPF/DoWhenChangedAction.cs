using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ControlsWPF
{

    [TypeConverter(typeof(DuoActionTypeConverter))]
    public class DoWhenChangedAction : IEquatable<DoWhenChangedAction>
    {

        //public Type ActionType { get; set; }
        public Delegate ActionDelegate { get; set; }

        //public DoWhenChangedAction() : this(typeof(object), null) {}
        public DoWhenChangedAction() : this(null) { }

        //public DoWhenChangedAction(Type actType, Delegate act)
        public DoWhenChangedAction(Delegate act)
        {
            //ActionType = actType;
            ActionDelegate = act;
        }

        public bool Equals(DoWhenChangedAction other)
        {
            // TODO: Use Type Convert to covert each and then compare the string version.
            if (other == null) return false;

            //if (other.ActionType == ActionType && other.ActionDelegate == ActionDelegate) return true;
            if (other.ActionDelegate == ActionDelegate) return true;

            return false;
        }
    }
}
