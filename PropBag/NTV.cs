using System;
using System.Runtime.CompilerServices;


namespace DRM.PropBag
{
    /// <summary>
    /// Vehicle to ferry Prop Values.
    /// Name, Type Value or NTV.
    /// </summary>
    public struct NTV
    {
        public string PropName { get; private set; }
        public Type PropType { get; private set; }
        public object Value { get; private set; }

        public NTV(object value, Type propType, [CallerMemberName]string propName = null)
            : this(propName, propType, value) {}

        public NTV(string propName, Type propType, object value) : this()
        {
            PropName = propName;
            PropType = propType;
            Value = value;
        }
    }
}
