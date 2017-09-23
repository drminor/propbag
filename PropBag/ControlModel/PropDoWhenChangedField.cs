using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ControlModel 
{
    public class PropDoWhenChangedField : NotifyPropertyChangedBase, IEquatable<PropDoWhenChangedField>
    {
        Delegate dwc;
        bool dan;

        // TODO: This is not Serializable, consider providing string representation as a proxy
        // Perhaps we should simply not serialize instances of PropBag Control Models.
        public Delegate DoWhenChangedAction { get { return dwc; } set { SetIfDifferentDelegate<Delegate>(ref dwc, value); } }

        public bool DoAfterNotify { get { return dan; } set { SetIfDifferent<bool>(ref dan, value); } }

        public PropDoWhenChangedField() : this(null) {}

        public PropDoWhenChangedField(Delegate doWhenChangedAction, bool doAfterNotify = false)
        {
            DoWhenChangedAction = doWhenChangedAction;
            DoAfterNotify = doAfterNotify;
        }

        public bool Equals(PropDoWhenChangedField other)
        {
            if (other == null) return false;

            if (other.DoAfterNotify == DoAfterNotify && other.DoWhenChangedAction == DoWhenChangedAction) return true;

            return false;
        }
    }
}
