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
        public Delegate DoWhenChanged { get; set; }

        #region Constructors

        public DoWhenChangedAction() : this(null) { }

        public DoWhenChangedAction(Delegate act)
        {
            DoWhenChanged = act;
        }

        #endregion

        public bool Equals(DoWhenChangedAction other)
        {
            // TODO: Use Type Convert to covert each and then compare the string version.
            if (other == null) return false;

            if (other.DoWhenChanged == DoWhenChanged) return true;

            return false;
        }
    }
}
