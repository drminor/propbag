using DRM.TypeSafePropertyBag.DelegateCaches;
using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    // TODO: Consider implementing IDisposable -- to clear the WeakRefKey(Target)
    internal class ParentNCSubscriptionRequest
    {
        #region Private Members

        Func<ParentNCSubscriptionRequest, ICacheDelegates<CallPSParentNodeChangedEventSubDelegate>, ParentNCSubscription> SubscriptionFactory { get; }

        #endregion

        #region Public Properties

        public ExKeyT OwnerPropId { get; }

        public WeakRefKey Target_Wrk { get; private set; }
        public object Target => Target_Wrk.Target;

        public MethodInfo Method { get; }

        public bool HasBeenUsed { get; private set; }

        #endregion

        #region Constructors for Property Changed Handlers

        public ParentNCSubscriptionRequest(ExKeyT sourcePropId, EventHandler<PSNodeParentChangedEventArgs> handler)
        {
            OwnerPropId = sourcePropId;

            object target = handler.Target ?? throw new ArgumentNullException(nameof(handler.Target));
            Target_Wrk = new WeakRefKey(target);
            Method = handler.Method ?? throw new ArgumentNullException(nameof(handler.Method));

            SubscriptionFactory = CreateSubscriptionGen;
            HasBeenUsed = false;
        }

        #endregion

        #region Public Methods

        public ParentNCSubscription CreateSubscription(ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> callPSParentNodeChangedEventSubsCache)
        {
            return SubscriptionFactory(this, callPSParentNodeChangedEventSubsCache);
        }

        private ParentNCSubscription CreateSubscriptionGen(ParentNCSubscriptionRequest request, ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> callPSParentNodeChangedEventSubsCache)
        {
            ParentNCSubscription result = new ParentNCSubscription(request, callPSParentNodeChangedEventSubsCache);

            request.MarkAsUsed();
            return result;
        }

        public void MarkAsUsed()
        {
            Target_Wrk = WeakRefKey.Empty; // This removes our reference to the underlying System.WeakRef value.
            HasBeenUsed = true;
        }

        #endregion
    }
}
