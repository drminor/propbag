using DRM.TypeSafePropertyBag;
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
    // : IEquatable<DoWhenChangedAction>
    {
        public EventHandler<PropertyChangedWithValsEventArgs> DoWhenChanged { get; set; }

        #region Constructors

        public DoWhenChangedAction() : this(null) { }

        public DoWhenChangedAction(EventHandler<PropertyChangedWithValsEventArgs> act)
        {
            DoWhenChanged = act;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DoWhenChangedAction);
        }

        public bool Equals(DoWhenChangedAction other)
        {
            return other != null &&
                   EqualityComparer<EventHandler<PropertyChangedWithValsEventArgs>>.Default.Equals(DoWhenChanged, other.DoWhenChanged);
        }

        public override int GetHashCode()
        {
            return 2044295968 + EqualityComparer<EventHandler<PropertyChangedWithValsEventArgs>>.Default.GetHashCode(DoWhenChanged);
        }

        public static bool operator ==(DoWhenChangedAction action1, DoWhenChangedAction action2)
        {
            return EqualityComparer<DoWhenChangedAction>.Default.Equals(action1, action2);
        }

        public static bool operator !=(DoWhenChangedAction action1, DoWhenChangedAction action2)
        {
            return !(action1 == action2);
        }

        #endregion
    }
}
