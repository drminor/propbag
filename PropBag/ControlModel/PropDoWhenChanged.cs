using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel 
{
    public class PropDoWhenChanged : NotifyPropertyChangedBase, IEquatable<PropDoWhenChanged>
    {
        string dwc;
        bool dan;

        [XmlText]
        public string DoWhenChanged { get { return dwc; } set { SetIfDifferent<string>(ref dwc, value); } }

        [XmlAttribute("do-after-notify")]
        public bool DoAfterNotify { get { return dan; } set { SetIfDifferent<bool>(ref dan, value); } }

        public PropDoWhenChanged() : this(null) {}

        public PropDoWhenChanged(string doWhenChangedDelegateName, bool doAfterNotify = false)
        {
            DoWhenChanged = doWhenChangedDelegateName;
            DoAfterNotify = doAfterNotify;
        }

        public bool Equals(PropDoWhenChanged other)
        {
            if (other == null) return false;

            if (other.DoAfterNotify == DoAfterNotify && other.DoWhenChanged == DoWhenChanged) return true;

            return false;
        }
    }
}
