using DRM.TypeSafePropertyBag.DelegateCaches;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public class SubscriptionKeyGen : ISubscriptionKeyGen, IEquatable<SubscriptionKeyGen>
    {
        #region Private Members

        Func<ISubscriptionKeyGen, IProvideHandlerDispatchDelegateCaches, ISubscription> SubscriptionFactory { get; }
        Func<ISubscriptionKeyGen, PSAccessServiceInterface, ISubscription> BindingFactory { get;}

        #endregion

        #region Public Properties

        public ExKeyT OwnerPropId { get; }
        public Type PropertyType { get; protected set; }

        public SubscriptionKind SubscriptionKind { get; }
        public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; }
        //public SubscriptionTargetKind SubscriptionTargetKind { get; }

        public WeakRefKey Target_Wrk { get; private set; }
        public object Target => Target_Wrk.Target;

        public MethodInfo Method { get; }

        //public Action<object, object> GenDoWhenChanged { get; private set; }
        //public Action Action { get; private set; }

        public bool HasBeenUsed { get; private set; }

        // Members for Binding Subscriptions
        public LocalBindingInfo BindingInfo { get; }

        #endregion

        #region Constructors for Property Changed Handlers

        // Standard PropertyChangedEventHandler
        public SubscriptionKeyGen(ExKeyT sourcePropId, PropertyChangedEventHandler standardDelegate,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
        {
            OwnerPropId = sourcePropId;
            PropertyType = null;

            SubscriptionKind = SubscriptionKind.StandardHandler;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;
            //SubscriptionTargetKind = GetKindOfTarget(standardDelegate.Target, keepRef);

            //GenDoWhenChanged = null;
            //Action = null;

            object target = standardDelegate.Target ?? throw new ArgumentNullException(nameof(standardDelegate.Target));
            Target_Wrk = new WeakRefKey(target);
            Method = standardDelegate.Method ?? throw new ArgumentNullException(nameof(standardDelegate.Method));

            SubscriptionFactory = CreateSubscriptionGen;
            BindingFactory = null;
            HasBeenUsed = false;
        }

        // Changing PropertyChangingEventHandler
        public SubscriptionKeyGen(ExKeyT sourcePropId, PropertyChangingEventHandler changingDelegate,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
        {
            OwnerPropId = sourcePropId;
            PropertyType = null;

            SubscriptionKind = SubscriptionKind.ChangingHandler;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;
            //SubscriptionTargetKind = GetKindOfTarget(standardDelegate.Target, keepRef);

            //GenDoWhenChanged = null;
            //Action = null;

            object target = changingDelegate.Target ?? throw new ArgumentNullException(nameof(changingDelegate.Target));
            Target_Wrk = new WeakRefKey(target);
            Method = changingDelegate.Method ?? throw new ArgumentNullException(nameof(changingDelegate.Method));

            SubscriptionFactory = CreateSubscriptionGen;
            BindingFactory = null;
            HasBeenUsed = false;
        }

        // PCGenEventArgs
        public SubscriptionKeyGen(ExKeyT sourcePropId, EventHandler<PcGenEventArgs> genDelegate,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
        {
            OwnerPropId = sourcePropId;
            PropertyType = null;

            SubscriptionKind = SubscriptionKind.GenHandler;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;
            //SubscriptionTargetKind = GetKindOfTarget(genDelegate.Target, keepRef);

            //GenDoWhenChanged = null;
            //Action = null;

            object target = genDelegate.Target ?? throw new ArgumentNullException(nameof(genDelegate.Target));
            Target_Wrk = new WeakRefKey(target);
            Method = genDelegate.Method ?? throw new ArgumentNullException(nameof(genDelegate.Method));

            SubscriptionFactory = CreateSubscriptionGen;
            BindingFactory = null;
            HasBeenUsed = false;
        }

        // PCObjEventArgs
        public SubscriptionKeyGen(ExKeyT sourcePropId, EventHandler<PcObjectEventArgs> objDelegate,
            SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
        {
            OwnerPropId = sourcePropId;
            PropertyType = null;

            SubscriptionKind = SubscriptionKind.ObjHandler;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;
            //SubscriptionTargetKind = GetKindOfTarget(objDelegate.Target, keepRef);

            //GenDoWhenChanged = null;
            //Action = null;

            object target = objDelegate.Target ?? throw new ArgumentNullException(nameof(objDelegate.Target));
            Target_Wrk = new WeakRefKey(target);
            Method = objDelegate.Method ?? throw new ArgumentNullException(nameof(objDelegate.Method));

            SubscriptionFactory = CreateSubscriptionGen;
            BindingFactory = null;
            HasBeenUsed = false;
        }

        // Target and Method. Also used for TypeDelegate and TypedAction.
        public SubscriptionKeyGen(ExKeyT sourcePropId, Type propertyType,
            object target, MethodInfo method,
            SubscriptionKind kind, 
            SubscriptionPriorityGroup subscriptionPriorityGroup,
            bool keepRef,
            Func<ISubscriptionKeyGen, IProvideHandlerDispatchDelegateCaches, ISubscription> subscriptionFactory)
        {
            OwnerPropId = sourcePropId;
            PropertyType = propertyType;

            SubscriptionKind = kind;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;
            //SubscriptionTargetKind = GetKindOfTarget(target, keepRef);

            //GenDoWhenChanged = null;
            //Action = null;

            if(target == null) throw new ArgumentNullException(nameof(target));
            Target_Wrk = new WeakRefKey(target);
            Method = method ?? throw new ArgumentNullException(nameof(method));

            SubscriptionFactory = subscriptionFactory ?? CreateSubscriptionGen;
            BindingFactory = null;
            HasBeenUsed = false;
        }

        #endregion

        #region Constructors for Actions

        // Action<object, object>
        protected SubscriptionKeyGen(ExKeyT sourcePropId, Action<object, object> genAction,
            SubscriptionPriorityGroup subscriptionPriorityGroup,
            bool keepRef,
            Func<ISubscriptionKeyGen, IProvideHandlerDispatchDelegateCaches, ISubscription> subscriptionFactory)
        {
            OwnerPropId = sourcePropId;
            PropertyType = null;

            SubscriptionKind = SubscriptionKind.ObjectAction;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;
            //SubscriptionTargetKind = GetKindOfTarget(genAction.Target, keepRef);

            //GenDoWhenChanged = genAction ?? throw new ArgumentNullException(nameof(genAction));
            //Action = null;

            object target = genAction.Target ?? throw new InvalidOperationException($"The value for Target on the GenAction action, cannot be null.");
            Target_Wrk = new WeakRefKey(target);
            Method = genAction.Method ?? throw new ArgumentNullException(nameof(genAction.Method));

            SubscriptionFactory = subscriptionFactory;
            BindingFactory = null;
            HasBeenUsed = false;
        }

        // ActionNoParams 
        protected SubscriptionKeyGen(ExKeyT sourcePropId, Action action,
            SubscriptionPriorityGroup subscriptionPriorityGroup,
            bool keepRef,
            Func<ISubscriptionKeyGen, IProvideHandlerDispatchDelegateCaches, ISubscription> subscriptionFactory)
        {
            OwnerPropId = sourcePropId;
            PropertyType = null;

            SubscriptionKind = SubscriptionKind.ActionNoParams;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;
            //SubscriptionTargetKind = GetKindOfTarget(action.Target, keepRef);

            //GenDoWhenChanged = null;
            //Action = action;

            object target = action.Target ?? throw new ArgumentNullException(nameof(action.Target));
            Target_Wrk = new WeakRefKey(target);
            Method = action.Method ?? throw new ArgumentNullException(nameof(action.Method));

            SubscriptionFactory = subscriptionFactory;
            BindingFactory = null;
            HasBeenUsed = false;
        }

        #endregion

        #region Constructors for Binding Subscriptions

        // Note since the Target and Method are not set, the default IEquatable implementation does not produce
        // accurate results.
        // DRM: Acutally the IEquatable implementation should be ok -- but need to create unit test to be sure. 
        // TODO: Create unit test to verify that the IEquatable implementation for SubscriptionKeyForBinding produces accurate result.

        // Creates a new Binding Request.
        protected SubscriptionKeyGen(
            ExKeyT ownerPropId,
            Type propertyType,
            LocalBindingInfo bindingInfo,
            SubscriptionKind kind,
            SubscriptionPriorityGroup subscriptionPriorityGroup,
            Func<ISubscriptionKeyGen, PSAccessServiceInterface, ISubscription> bindingFactory)
        {
            OwnerPropId = ownerPropId; // The binding is created on the target, we will go find the source of the events to listen.
            PropertyType = PropertyType;

            SubscriptionKind = kind;
            SubscriptionPriorityGroup = subscriptionPriorityGroup;
            //SubscriptionTargetKind = SubscriptionTargetKind.GlobalPropId;

            //GenDoWhenChanged = null;
            //Action = null;

            Target_Wrk = WeakRefKey.Empty;
            Method = null;

            SubscriptionFactory = null;
            BindingFactory = bindingFactory;
            HasBeenUsed = false;

            // Properties unique to Binding Subscriptions
            BindingInfo = bindingInfo;
        }

        #endregion

        #region Private Methods / Constructor support

        //private SubscriptionTargetKind GetKindOfTarget(object target, bool keepRef)
        //{
        //    if(target.GetType().IsPropBagBased())
        //    {
        //        return SubscriptionTargetKind.PropBag;
        //    }
        //    else
        //    {
        //        return keepRef ? SubscriptionTargetKind.StandardKeepRef : SubscriptionTargetKind.Standard;
        //    }
        //}

        #endregion

        #region Public Methods

        public ISubscription CreateSubscription(IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            return SubscriptionFactory(this, handlerDispatchDelegateCacheProvider);
        }

        private ISubscription CreateSubscriptionGen(ISubscriptionKeyGen subscriptionRequestGen, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            ISubscription result = new SubscriptionGen(subscriptionRequestGen, handlerDispatchDelegateCacheProvider);
            subscriptionRequestGen.MarkAsUsed();
            return result;
        }

        public virtual ISubscription CreateBinding(PSAccessServiceInterface propStoreAccessService)
        {
            return BindingFactory(this, propStoreAccessService);
        }

        public void MarkAsUsed()
        {
            Target_Wrk = WeakRefKey.Empty; // This removes our reference to the underlying System.WeakRef value.

            //GenDoWhenChanged = null;
            //Action = null;

            HasBeenUsed = true;
        }

        #endregion

        #region IEquatable Support and Object Overrides

        public override string ToString()
        {
            return $"SubscriptionKeyGen for {OwnerPropId} of kind = {SubscriptionKind} with target object: {Target.GetType().Name} and method: {Method.Name}.";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SubscriptionKeyGen);
        }

        public bool Equals(SubscriptionKeyGen other)
        {
            return other != null &&
                   OwnerPropId == other.OwnerPropId &&
                   EqualityComparer<object>.Default.Equals(Target, other.Target) &&
                   EqualityComparer<MethodInfo>.Default.Equals(Method, other.Method);
        }

        public override int GetHashCode()
        {
            var hashCode = 1273468457;
            hashCode = hashCode * -1521134295 + OwnerPropId.GetHashCode();
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
