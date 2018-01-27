﻿using DRM.TypeSafePropertyBag.Fundamentals;
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
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public class ParentNCSubscriptionRequest
    {
        #region Private Members

        Func<ParentNCSubscriptionRequest, ICacheDelegates<CallPSParentNodeChangedEventSubDelegate>, ParentNCSubscription> SubscriptionFactory { get; }

        #endregion

        #region Public Properties

        public ExKeyT OwnerPropId { get; }

        public object Target { get; private set; } 
        public MethodInfo Method { get; }

        public bool HasBeenUsed { get; private set; }

        #endregion

        #region Constructors for Property Changed Handlers

        public ParentNCSubscriptionRequest(ExKeyT sourcePropId, EventHandler<PSNodeParentChangedEventArgs> handler)
        {
            OwnerPropId = sourcePropId;

            Target = handler.Target ?? throw new ArgumentNullException(nameof(handler.Target));
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
            Target = null;
            HasBeenUsed = true;
        }

        #endregion
    }
}
