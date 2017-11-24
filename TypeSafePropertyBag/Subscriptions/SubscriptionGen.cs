using System;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class SubscriptionGen : ISubscriptionGen
    {
        public ExKeyT SourcePropRef { get; protected set; }

        public SubscriptionKind SubscriptionKind { get; protected set; }
        public SubscriptionTargetKind SubscriptionTargetKind { get; protected set; }
        public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; protected set; }

        public Type PropertyType { get; }

        public EventHandler<PCGenEventArgs> GenHandler { get; protected set; }
        public EventHandler<PCObjectEventArgs> ObjHandler { get; protected set; }

        public EventHandler<PropertyChangedEventArgs> StandardHandler { get; protected set; }

        public Action<object, object> GenDoWhenChanged { get; protected set; }
        public Action Action { get; protected set; }

        public object Target { get; protected set; }
        public MethodInfo Method { get; protected set; }

        // Binding Subscription Members
        public ExKeyT TargetPropRef { get; protected set; }
        public LocalBindingInfo BindingInfo { get; protected set; }

        public object LocalBinderRefProxy => null;

        public SubscriptionGen(ISubscriptionKeyGen sKey)
        {
            PropertyType = sKey.PropertyType;
            SourcePropRef = sKey.SourcePropRef;

            GenHandler = sKey.GenHandler;
            ObjHandler = sKey.ObjHandler;
            StandardHandler = sKey.StandardHandler;
            GenDoWhenChanged = sKey.GenDoWhenChanged;
            Action = sKey.Action;

            Target = sKey.Target;
            Method = sKey.Method;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            SubscriptionTargetKind = sKey.SubscriptionTargetKind;
        }
    }
}
