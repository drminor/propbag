using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    public class SubscriptionKeyGen : ISubscriptionKeyGen, IEquatable<SubscriptionKeyGen>
    {
        #region Private Members

        Func<ISubscriptionKeyGen, ISubscriptionGen> SubscriptionCreator { get; }
        Func<ISubscriptionKeyGen, PSAccessServiceType, ISubscriptionGen> BindingFactory { get;}

        #endregion

        #region Public Properties

        public Type PropertyType { get; }
        public ExKeyT SourcePropRef { get; }

        public SubscriptionKind SubscriptionKind { get; }
        public SubscriptionTargetKind SubscriptionTargetKind { get; }
        public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; }

        public bool UseTargetAndMethod { get; }

        public EventHandler<PCGenEventArgs> GenHandler { get; private set; }
        public EventHandler<PCObjectEventArgs> ObjHandler { get; private set; }

        public EventHandler<PropertyChangedEventArgs> StandardHandler { get; private set; }

        public object Target { get; private set; } // For BindingSubscriptions this will always be a WeakReference<IPropBag>
        public MethodInfo Method { get; }

        public Action<object, object> GenDoWhenChanged { get; private set; }
        public Action Action { get; private set; }

        public bool HasBeenUsed { get; private set; }

        // Members for Binding Subscriptions
        public ExKeyT TargetPropRef { get; }
        public LocalBindingInfo BindingInfo { get; }

        #endregion

        #region Constructors for Property Changed Handlers

        // Standard PropertyChanged
        public SubscriptionKeyGen(ExKeyT sourcePropId, EventHandler<PropertyChangedEventArgs> standardDelegate,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
        {
            SourcePropRef = sourcePropId;
            SubscriptionKind = SubscriptionKind.StandardHandler;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            this.
            GenHandler = null;
            ObjHandler = null;
            StandardHandler = standardDelegate;
            UseTargetAndMethod = false;
            GenDoWhenChanged = null;
            Action = null;
            Target = standardDelegate.Target;
            Method = standardDelegate.Method;

            SubscriptionTargetKind = GetKindOfTarget(standardDelegate.Target, keepRef);
            SubscriptionCreator = CreateSubscriptionGen;
            HasBeenUsed = false;
        }

        // PCGenEventArgs
        public SubscriptionKeyGen(ExKeyT sourcePropId, EventHandler<PCGenEventArgs> genDelegate,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
        {
            SourcePropRef = sourcePropId;
            SubscriptionKind = SubscriptionKind.GenHandler;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            StandardHandler = null;
            GenHandler = genDelegate;
            ObjHandler = null;

            UseTargetAndMethod = false;
            GenDoWhenChanged = null;
            Action = null;
            Target = genDelegate.Target;
            Method = genDelegate.Method;

            SubscriptionTargetKind = GetKindOfTarget(genDelegate.Target, keepRef);
            SubscriptionCreator = CreateSubscriptionGen;
            HasBeenUsed = false;
        }

        // PCObjEventArgs
        public SubscriptionKeyGen(ExKeyT sourcePropId, EventHandler<PCObjectEventArgs> objDelegate,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
        {
            SourcePropRef = sourcePropId;
            SubscriptionKind = SubscriptionKind.ObjHandler;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            StandardHandler = null;
            GenHandler = null;
            ObjHandler = objDelegate;
            UseTargetAndMethod = false;
            GenDoWhenChanged = null;
            Action = null;
            Target = objDelegate.Target;
            Method = objDelegate.Method;

            SubscriptionTargetKind = GetKindOfTarget(objDelegate.Target, keepRef);
            SubscriptionCreator = CreateSubscriptionGen;
            HasBeenUsed = false;
        }

        // Target and Method. Also used for TypeDelegate and TypedAction.
        public SubscriptionKeyGen(ExKeyT sourcePropId, object target, MethodInfo method,
            SubscriptionKind kind, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef, Func<ISubscriptionKeyGen, ISubscriptionGen> subscriptionCreator)
        {
            SourcePropRef = sourcePropId;
            SubscriptionKind = kind;
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

        #endregion

        #region Constructors for Actions

        // Action<object, object>
        protected SubscriptionKeyGen(ExKeyT sourcePropId, Action<object, object> genAction,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef, Func<ISubscriptionKeyGen, ISubscriptionGen> subscriptionCreator)
        {
            SourcePropRef = sourcePropId;
            SubscriptionKind = SubscriptionKind.ObjectAction;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            StandardHandler = null;
            GenHandler = null;
            UseTargetAndMethod = false;
            GenDoWhenChanged = genAction ?? throw new ArgumentNullException(nameof(genAction));
            Action = null;
            Target = genAction.Target ?? throw new InvalidOperationException($"The value for Target on the GenAction action, cannot be null.");
            Method = genAction.Method;

            SubscriptionTargetKind = GetKindOfTarget(genAction.Target, keepRef);
            SubscriptionCreator = subscriptionCreator;
            HasBeenUsed = false;
        }

        // ActionNoParams 
        protected SubscriptionKeyGen(ExKeyT sourcePropId, Action action,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef, Func<ISubscriptionKeyGen, ISubscriptionGen> subscriptionCreator)
        {
            SourcePropRef = sourcePropId;
            SubscriptionKind = SubscriptionKind.ActionNoParams;
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

        #region Constructors for Binding Subscriptions

        // TODO: Note since the Target and Method are not set, the default IEquatable implementation does not produce
        // accurate results.

        // Creates a new Binding Request.
        protected SubscriptionKeyGen(
            Type propertyType,
            ExKeyT targetPropRef, 
            LocalBindingInfo bindingInfo,
            SubscriptionKind kind,
            SubscriptionPriorityGroup subscriptionPriorityGroup,
            Func<ISubscriptionKeyGen, PSAccessServiceType, ISubscriptionGen> bindingFactory)
        {
            SubscriptionKind = kind;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;

            StandardHandler = null;
            GenHandler = null;
            ObjHandler = null;

            UseTargetAndMethod = true;
            GenDoWhenChanged = null;
            Action = null;
            Target = null;
            Method = null;

            SubscriptionTargetKind = SubscriptionTargetKind.LocalWeakRef;
            SubscriptionCreator = null;
            BindingFactory = bindingFactory;
            HasBeenUsed = false;

            // Properties unique to Binding Subscriptions
            TargetPropRef = targetPropRef; // The binding is created on the target, we will go find the source of the events to listen.
            BindingInfo = bindingInfo;
        }

        #endregion

        #region Private Methods / Constructor support

        private SubscriptionTargetKind GetKindOfTarget(object target, bool keepRef)
        {
            if(target.GetType().IsPropBagBased())
            {
                return SubscriptionTargetKind.PropBag;
            }
            else
            {
                return keepRef ? SubscriptionTargetKind.StandardKeepRef : SubscriptionTargetKind.Standard;
            }
        }

        public static ISubscriptionGen CreateSubscriptionGen(ISubscriptionKeyGen subscriptionRequestGen)
        {
            ISubscriptionGen result = new SubscriptionGen(subscriptionRequestGen);
            return result;
        }

        #endregion

        #region Public Methods

        public ISubscriptionGen CreateSubscription()
        {
            return SubscriptionCreator(this);
        }

        public virtual ISubscriptionGen CreateBinding(PSAccessServiceType propStoreAccessService)
        {
            return BindingFactory(this, propStoreAccessService);
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

        #endregion

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as SubscriptionKeyGen);
        }

        public bool Equals(SubscriptionKeyGen other)
        {
            return other != null &&
                   SourcePropRef == other.SourcePropRef &&
                   EqualityComparer<object>.Default.Equals(Target, other.Target) &&
                   EqualityComparer<MethodInfo>.Default.Equals(Method, other.Method);
        }

        public override int GetHashCode()
        {
            var hashCode = 1273468457;
            hashCode = hashCode * -1521134295 + SourcePropRef.GetHashCode();
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
