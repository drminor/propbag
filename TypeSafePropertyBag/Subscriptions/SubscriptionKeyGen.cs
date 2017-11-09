using DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public class SubscriptionKeyGen : ISubscriptionKeyGen, IEquatable<SubscriptionKeyGen>
    {
        #region Private Members

        Func<ISubscriptionKeyGen, ISubscriptionGen> SubscriptionCreator { get; }

        #endregion

        #region Public Members

        public IExplodedKey<ulong, uint, uint> ExKey { get; }

        public SubscriptionKind SubscriptionKind { get; }
        public SubscriptionTargetKind SubscriptionTargetKind { get; }
        public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; }

        public bool UseTargetAndMethod { get; }

        public EventHandler<PCGenEventArgs> GenHandler { get; private set; }
        public EventHandler<PropertyChangedEventArgs> StandardHandler { get; private set; }

        public object Target { get; private set; }
        public MethodInfo Method { get; }

        public Action<object, object> GenDoWhenChanged { get; private set; }
        public Action Action { get; private set; }

        public bool HasBeenUsed { get; private set; }

        #endregion

        #region Constructors

        // Standard PropertyChanged
        protected SubscriptionKeyGen(SimpleExKey exKey, EventHandler<PropertyChangedEventArgs> standardDelegate,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef, Func<ISubscriptionKeyGen, ISubscriptionGen> subscriptionCreator)
        {
            ExKey = exKey;
            SubscriptionKind = SubscriptionKind.StandardHandler;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            StandardHandler = standardDelegate;
            UseTargetAndMethod = false;
            GenDoWhenChanged = null;
            Action = null;
            Target = standardDelegate.Target;
            Method = standardDelegate.Method;

            SubscriptionTargetKind = GetKindOfTarget(standardDelegate.Target, keepRef);
            SubscriptionCreator = subscriptionCreator;
            HasBeenUsed = false;
        }

        // PCGenEventArgs
        protected SubscriptionKeyGen(SimpleExKey exKey, EventHandler<PCGenEventArgs> genDelegate,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef, Func<ISubscriptionKeyGen, ISubscriptionGen> subscriptionCreator)
        {
            ExKey = exKey;
            SubscriptionKind = SubscriptionKind.StandardHandler;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            StandardHandler = null;
            GenHandler = genDelegate;
            UseTargetAndMethod = false;
            GenDoWhenChanged = null;
            Action = null;
            Target = genDelegate.Target;
            Method = genDelegate.Method;

            SubscriptionTargetKind = GetKindOfTarget(genDelegate.Target, keepRef);
            SubscriptionCreator = subscriptionCreator;
            HasBeenUsed = false;
        }

        // Target and Method. Also used for TypeDelegate and TypedAction.
        protected SubscriptionKeyGen(SimpleExKey exKey, object target, MethodInfo method,
            SubscriptionKind kind, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef, Func<ISubscriptionKeyGen, ISubscriptionGen> subscriptionCreator)
        {
            ExKey = exKey;
            SubscriptionKind = kind;
            SubscriptionTargetKind = SubscriptionTargetKind.Standard;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            StandardHandler = null;
            GenHandler = null;
            UseTargetAndMethod = true;
            GenDoWhenChanged = null;
            Action = null;
            Target = target;
            Method = method;

            SubscriptionTargetKind = GetKindOfTarget(target, keepRef);
            SubscriptionCreator = subscriptionCreator;
            HasBeenUsed = false;
        }

        // Action<object, object>
        protected SubscriptionKeyGen(SimpleExKey exKey, Action<object, object> genAction,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef, Func<ISubscriptionKeyGen, ISubscriptionGen> subscriptionCreator)
        {
            ExKey = exKey;
            SubscriptionKind = SubscriptionKind.ObjectAction;
            SubscriptionTargetKind = SubscriptionTargetKind.Standard;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            StandardHandler = null;
            GenHandler = null;
            UseTargetAndMethod = false;
            GenDoWhenChanged = genAction;
            Action = null;
            Target = genAction.Target;
            Method = genAction.Method;

            SubscriptionTargetKind = GetKindOfTarget(genAction.Target, keepRef);
            SubscriptionCreator = subscriptionCreator;
            HasBeenUsed = false;
        }

        // ActionNoParams 
        protected SubscriptionKeyGen(SimpleExKey exKey, Action action,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef, Func<ISubscriptionKeyGen, ISubscriptionGen> subscriptionCreator)
        {
            ExKey = exKey;
            SubscriptionKind = SubscriptionKind.ActionNoParams;
            SubscriptionTargetKind = SubscriptionTargetKind.Standard;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            StandardHandler = null;
            GenHandler = null;
            UseTargetAndMethod = false;
            GenDoWhenChanged = null;
            Action = action;
            Target = action.Target;
            Method = action.Method;

            SubscriptionTargetKind = GetKindOfTarget(action.Target, keepRef);
            SubscriptionCreator = subscriptionCreator;
            HasBeenUsed = false;
        }

        #endregion

        private SubscriptionTargetKind GetKindOfTarget(object target, bool keepRef)
        {
            if(target is IPropBag)
            {
                return SubscriptionTargetKind.PropBag;
            }
            else
            {
                return keepRef ? SubscriptionTargetKind.StandardKeepRef : SubscriptionTargetKind.Standard;
            }
        }

        public ISubscriptionGen CreateSubscription()
        {
            return SubscriptionCreator(this);
        }

        public void MarkAsUsed()
        {
            GenHandler = null;
            StandardHandler = null;
            Target = null;
            GenDoWhenChanged = null;
            Action = null;

            HasBeenUsed = true;
        }

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as SubscriptionKeyGen);
        }

        public bool Equals(SubscriptionKeyGen other)
        {
            return other != null &&
                   ExKey == other.ExKey &&
                   EqualityComparer<object>.Default.Equals(Target, other.Target) &&
                   EqualityComparer<MethodInfo>.Default.Equals(Method, other.Method);
        }

        public override int GetHashCode()
        {
            var hashCode = 1273468457;
            hashCode = hashCode * -1521134295 + ExKey.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Target);
            hashCode = hashCode * -1521134295 + EqualityComparer<MethodInfo>.Default.GetHashCode(Method);
            return hashCode;
        }

        public static bool operator ==(SubscriptionKeyGen gen1, SubscriptionKeyGen gen2)
        {
            return EqualityComparer<SubscriptionKeyGen>.Default.Equals(gen1, gen2);
        }

        public static bool operator !=(SubscriptionKeyGen gen1, SubscriptionKeyGen gen2)
        {
            return !(gen1 == gen2);
        }

        #endregion
    }
}
