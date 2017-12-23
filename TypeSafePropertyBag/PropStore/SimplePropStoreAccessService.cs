﻿using System;
using System.Collections.Generic;
using System.Linq;

using DRM.TypeSafePropertyBag.Fundamentals;
using System.Collections.Concurrent;
using System.Reflection;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    using PSCloneServiceType = IProvidePropStoreCloneService<UInt32, String>;

    internal class SimplePropStoreAccessService : PSAccessServiceType, IHaveTheStoreNode, IDisposable
    {
        #region Private Members

        readonly WeakReference<SimplePropStoreAccessService> _wrThis;
        readonly WeakReference<IPropBagInternal> _clientAccessToken;
        readonly ObjectIdType _objectId;

        readonly StoreNodeBag _ourNode;

        PSAccessServiceProviderType _propStoreAccessServiceProvider;
        readonly IProvideHandlerDispatchDelegateCaches _handlerDispatchDelegateCacheProvider;

        //// Subscription Management
        //const int OBJECT_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        //const int EXPECTED_NO_OF_OBJECTS = 10000;

        private CollectionOfSubscriberCollections _propIndexes;

        readonly BindingsCollection _bindings;

        #endregion

        #region Constructor

        public SimplePropStoreAccessService
            (
            StoreNodeBag ourNode,
            L2KeyManType level2KeyManager,
            PSAccessServiceProviderType propStoreAccessServiceProvider,
            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider
            )
        {
            _wrThis = new WeakReference<SimplePropStoreAccessService>(this);
            _ourNode = ourNode;
            _propStoreAccessServiceProvider = propStoreAccessServiceProvider;
            _handlerDispatchDelegateCacheProvider = handlerDispatchDelegateCacheProvider;

            if(!(propStoreAccessServiceProvider is PSCloneServiceType))
            {
                throw new ArgumentException($"The {nameof(propStoreAccessServiceProvider)} does not implement the {nameof(PSCloneServiceType)} interface.");
            }

            Level2KeyManager = level2KeyManager;
            MaxObjectsPerAppDomain = propStoreAccessServiceProvider.MaxObjectsPerAppDomain;

            _clientAccessToken = _ourNode.PropBagProxy.PropBagRef;
            _objectId = _ourNode.CompKey.Level1Key;

            // Create the subscription store for this PropBag.
            _propIndexes = new CollectionOfSubscriberCollections();

            // Create the binding store for this PropBag.
            _bindings = new BindingsCollection();
        }

        #endregion

        #region Public Members

        public L2KeyManType Level2KeyManager { get; private set; }

        public int MaxPropsPerObject => Level2KeyManager.MaxPropsPerObject;
        public long MaxObjectsPerAppDomain { get; }

        public IPropData this[IPropBag propBag, PropIdType propId]
        {
            get
            {
                ExKeyT cKey = GetCompKey(propBag, propId);
                IPropData result = GetChild(cKey).Int_PropData;
                return result;
            }
            //private set
            //{
            //    IPropDataInternal int_PropData = GetInt_PropData(value);
            //    ExKeyT cKey = GetCompKey(propBag, propId);

            //    _ourNode.ChildList[cKey].Int_PropData = int_PropData;
            //}
        }

        public bool TryGetValue(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            ExKeyT cKey = GetCompKey(propBag, propId);
            if(_ourNode.TryGetChild(cKey, out StoreNodeProp child))
            {
                propData = child.Int_PropData;
                return true;
            }
            else
            {
                propData = null;
                return false;
            }
        }

        public bool TryAdd(IPropBag propBag, PropIdType propId, IProp genericTypedProp, out IPropData propData)
        {
            StoreNodeProp newNode = TryAddFirstPart(propBag, propId, genericTypedProp);
            propData = newNode.Int_PropData;

            TryAddSecondPart((IPropDataInternal)propData, newNode);

            return true;
        }

        public bool TryAdd<PropT>(IPropBag propBag, PropIdType propId, IProp genericTypedProp,
            EventHandler<PcTypedEventArgs<PropT>> eventHandler, SubscriptionPriorityGroup priorityGroup, out IPropData propData)
        {
            StoreNodeProp newNode = TryAddFirstPart(propBag, propId, genericTypedProp);
            propData = newNode.Int_PropData;

            bool result;
            if (eventHandler != null)
            {
                IDisposable disable = RegisterHandler<PropT>(newNode.CompKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                result = disable != null;
            }
            else
            {
                result = true;
            }

            TryAddSecondPart((IPropDataInternal)propData, newNode);

            return result;
        }

        public bool TryAdd(IPropBag propBag, PropIdType propId, IProp genericTypedProp,
            object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, out IPropData propData)
        {
            StoreNodeProp newNode = TryAddFirstPart(propBag, propId, genericTypedProp);
            propData = newNode.Int_PropData;

            bool result;
            if (target != null)
            {
                ISubscriptionKeyGen subscriptionRequest = 
                    new SubscriptionKeyGen(newNode.CompKey, genericTypedProp.Type, target, method, subscriptionKind, priorityGroup, keepRef: false, subscriptionFactory: null);

                ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);

                result = wasAdded;
            }
            else
            {
                result = true;
            }

            TryAddSecondPart((IPropDataInternal)propData, newNode);

            return result;
        }


        public bool TryAdd(IPropBag propBag, PropIdType propId, IProp genericTypedProp,
            EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, out IPropData propData)
        {
            //ExKeyT propertyKey = GetCompKey(propBag, propId);
            //propData = new PropGen(propertyKey, genericTypedProp);
            //IPropDataInternal int_PropData = (IPropDataInternal)propData;

            //StoreNodeProp propStoreNode = new StoreNodeProp(propertyKey, int_PropData, _ourNode);

            StoreNodeProp newNode = TryAddFirstPart(propBag, propId, genericTypedProp);
            propData = newNode.Int_PropData;

            bool result;
            if(eventHandler != null)
            {
                IDisposable disable = RegisterHandler(newNode.CompKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                result = disable != null;
            }
            else
            {
                result = true;
            }

            TryAddSecondPart((IPropDataInternal)propData, newNode);

            return result;
        }

        private void TryAddSecondPart(IPropDataInternal int_propData, StoreNodeProp propStoreNode)
        {
            if (int_propData.IsPropBag)
            {
                // If the new property is of a type that implements IPropBag,
                // attempt to get a reference to the StoreAccessor of that IPropBag object,
                // and from that StoreAccessor, the object id of the PropBag.
                // Use the PropBag's ObjectId to set the ChildObjectId of this new property.

                object guest = int_propData?.TypedProp?.TypedValueAsObject;
                if (guest != null)
                {
                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal((IPropBag)guest);
                    guestPropBagNode.Parent = propStoreNode;
                }

                // Subscribe to changes to this PropData's Value.
                BeginWatchingParent(propStoreNode.CompKey);
            }
        }

        private StoreNodeProp TryAddFirstPart(IPropBag propBag, PropIdType propId, IProp genericTypedProp)
        {
            ExKeyT propertyKey = GetCompKey(propBag, propId);
            IPropData propData = new PropGen(propertyKey, genericTypedProp);
            IPropDataInternal int_PropData = (IPropDataInternal)propData;

            StoreNodeProp propStoreNode = new StoreNodeProp(propertyKey, int_PropData, _ourNode);

            return propStoreNode;
        }

        public bool TryRemove(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            ExKeyT cKey = GetCompKey(propBag, propId);

            bool result = _ourNode.TryRemoveChild(cKey, out StoreNodeProp child);

            if (result)
                propData = child.Int_PropData;
            else
                propData = null;

            return result;
        }
         
        public bool ContainsKey(IPropBag propBag, PropIdType propId)
        {
            ExKeyT cKey = GetCompKey(propBag, propId);
            bool result = _ourNode.ChildExists(cKey);
            return result;
        }

        public IEnumerable<KeyValuePair<PropNameType, IPropData>> GetCollection(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);

            Dictionary<PropNameType, IPropData> result = _ourNode.Children.ToDictionary
                (
                x => GetPropNameFromKey(x.CompKey),
                x => (IPropData) x.Int_PropData
                );

            return result;
        }

        public IEnumerator<KeyValuePair<PropNameType, IPropData>> GetEnumerator(IPropBag propBag) 
        {
            GetAndCheckObjectRef(propBag);
            IEnumerator<KeyValuePair<PropNameType, IPropData>> result = GetCollection(propBag).GetEnumerator();
            return result;
        }

        public IEnumerable<PropNameType> GetKeys(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);
            IEnumerable<PropNameType> result = _ourNode.Children.Select(x => GetPropNameFromKey(x.CompKey));
            return result;
        }

        public IEnumerable<IPropData> GetValues(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);
            IEnumerable<IPropData> result = _ourNode.Children.Select(x => x.Int_PropData);
            return result;
        }

        // TODO: transfer subscriptions and bindings, if any, to the new PropItem.
        public bool SetTypedProp(IPropBag propBag, PropIdType propId, PropNameType propertyName, IProp genericTypedProp)
        {
            ExKeyT cKey = GetCompKey(propBag, propId);

            if (TryGetValue(propBag, propId, out IPropData propData))
            {
                IPropDataInternal int_propData = (IPropDataInternal)propData;

                IProp oldTypedProp = int_propData.TypedProp;

                if (!object.ReferenceEquals(oldTypedProp, genericTypedProp))
                {
                    if (oldTypedProp.TypedValueAsObject != null && oldTypedProp.TypedValueAsObject.GetType().IsPropBagBased())
                    {
                        // Remove the PropData's event handler from the PropBag event.
                        StopWatchingParent(cKey);
                    }

                    int_propData.SetTypedProp(propertyName, genericTypedProp);

                    if (genericTypedProp.TypedValueAsObject != null && genericTypedProp.TypedValueAsObject.GetType().IsPropBagBased())
                    {
                        // Subscribe to the new value's PropertyChanged event.
                        BeginWatchingParent(cKey);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public int ClearAllProps(IPropBag propBag)
        {
            int result = 0;

            return result;
        }

        #endregion

        #region IRegisterSubscriptions Implementation

        #region PC Typed Event Args

        public IDisposable RegisterHandler<T>(IPropBag propBag, PropIdType propId, 
            EventHandler<PcTypedEventArgs<T>> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetExKey(propBag, propId);

            IDisposable disable = RegisterHandler<T>(exKey, eventHandler, priorityGroup, keepRef);
            return disable;
        }

        private IDisposable RegisterHandler<T>(ExKeyT propertyId, EventHandler<PcTypedEventArgs<T>> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKey<T>(propertyId, eventHandler, priorityGroup, keepRef);
            ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);

            RemoveSub disable = new RemoveSub(_wrThis, subscriptionRequest);
            return disable;
        }

        public bool UnregisterHandler<T>(IPropBag propBag, PropIdType propId, EventHandler<PcTypedEventArgs<T>> eventHandler)
        {
            ExKeyT exKey = GetExKey(propBag, propId);

            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKey<T>(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region PC Gen Event Args

        public IDisposable RegisterHandler(IPropBag propBag, uint propId, 
            EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT propertyId = GetExKey(propBag, propId);
            IDisposable result = RegisterHandler(propertyId, eventHandler, priorityGroup, keepRef);
            return result;
        }

        private IDisposable RegisterHandler(ExKeyT propertyId, EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyId, eventHandler, priorityGroup, keepRef);
            ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);

            RemoveSub disable = new RemoveSub(_wrThis, subscriptionRequest);
            return disable;
        }

        public bool UnregisterHandler(IPropBag propBag, uint propId, EventHandler<PcGenEventArgs> eventHandler)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        public class RemoveSub : IDisposable
        {
            WeakReference<SimplePropStoreAccessService> _wr;
            ISubscriptionKeyGen _request;
            public RemoveSub(WeakReference<SimplePropStoreAccessService> wr_us, ISubscriptionKeyGen request)
            {
                _wr = wr_us;
                _request = request;
            }

            #region IDisposable Support

            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).
                        if(_wr.TryGetTarget(out SimplePropStoreAccessService accService))
                        {
                            accService.RemoveSubscription(_request);
                        }
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            // ~Temp() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }

            #endregion
        }

        #endregion

        #region PC Object Event Args

        public bool RegisterHandler(IPropBag propBag, uint propId, 
            EventHandler<PcObjectEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            bool wasAdded = RegisterHandler(exKey, eventHandler, priorityGroup, keepRef);
            return wasAdded;
        }

        private bool RegisterHandler(ExKeyT propertyId, EventHandler<PcObjectEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyId, eventHandler, priorityGroup, keepRef);
            ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);
            return wasAdded;
        }

       public bool UnregisterHandler(IPropBag propBag, uint propId, EventHandler<PcObjectEventArgs> eventHandler)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region PC Standard Event Args

        public bool RegisterHandler(IPropBag propBag, uint propId,
            PropertyChangedEventHandler eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            bool wasAdded = RegisterHandler(exKey, eventHandler, priorityGroup, keepRef);
            return wasAdded;
        }

        private bool RegisterHandler(ExKeyT propertyId, PropertyChangedEventHandler eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyId, eventHandler, priorityGroup, keepRef);
            ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);
            return wasAdded;
        }

        public bool UnregisterHandler(IPropBag propBag, uint propId, PropertyChangedEventHandler eventHandler)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region PC Changing Event Args

        public bool RegisterHandler(IPropBag propBag, uint propId,
            PropertyChangingEventHandler eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            bool wasAdded = RegisterHandler(exKey, eventHandler, priorityGroup, keepRef);
            return wasAdded;
        }

        private bool RegisterHandler(ExKeyT propertyId, PropertyChangingEventHandler eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyId, eventHandler, priorityGroup, keepRef);
            ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);
            return wasAdded;
        }

        public bool UnregisterHandler(IPropBag propBag, uint propId, PropertyChangingEventHandler eventHandler)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region Target and Method

        public bool RegisterHandler(IPropBag propBag, uint propId, Type propertyType, object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, propertyType, target, method, subscriptionKind, priorityGroup, keepRef, subscriptionFactory: null);

            ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);
            return wasAdded;
        }

        public bool UnregisterHandler(IPropBag propBag, uint propId, Type propertyType, object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, propertyType, target, method, subscriptionKind, priorityGroup, keepRef, subscriptionFactory: null);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #endregion

        #region Subscription Management

        public ISubscription AddSubscription(ISubscriptionKeyGen subscriptionRequest, out bool wasAdded)
        {
            if (subscriptionRequest.HasBeenUsed)
            {
                throw new ApplicationException("Its already been used.");
            }

            SubscriberCollection sc = GetOrAddSubscriberCollection(subscriptionRequest.OwnerPropId);

            ISubscription result = sc.GetOrAdd
                (
                subscriptionRequest,
                    (
                    x => subscriptionRequest.CreateSubscription(_handlerDispatchDelegateCacheProvider)
                    )
                );

            if (subscriptionRequest.HasBeenUsed)
            {
                System.Diagnostics.Debug.WriteLine($"Created a new Subscription for Property:" +
                    $" {subscriptionRequest.OwnerPropId} / Event: {result.SubscriptionKind}.");
                wasAdded = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"The subscription for Property:" +
                    $" {subscriptionRequest.OwnerPropId} / Event: {result.SubscriptionKind} was not added.");
                wasAdded = false;
            }
            return result;
        }

        public bool RemoveSubscription(ISubscriptionKeyGen subscriptionRequest)
        {
            if (TryGetSubscriptions(subscriptionRequest.OwnerPropId, out SubscriberCollection sc))
            {
                bool result = sc.TryRemoveSubscription(subscriptionRequest);

                if (result)
                    System.Diagnostics.Debug.WriteLine($"Removed the subscription for {subscriptionRequest.OwnerPropId}.");

                return result;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<ISubscription> GetSubscriptions(IPropBag host, PropIdType propId)
        {
            //ExKeyT exKey = GetExKey(host, propId);

            // TODO: NOTE: This does not verify that the caller is the "correct" one.
            ExKeyT exKey = new SimpleExKey(_objectId, propId);

            if (TryGetSubscriptions(exKey, out IEnumerable<ISubscription> subs))
            {
                return subs;
            }
            else
            {
                IEnumerable<ISubscription> result = new SubscriberCollection();
                return result;
            }
        }

        public bool TryGetSubscriptions(ExKeyT exKey, out IEnumerable<ISubscription> subscriberCollection)
        {
            bool result = _propIndexes.TryGetSubscriberCollection(exKey.Level2Key, out subscriberCollection);

            //bool gResult = _propIndexes.TryGetSubscriberCollection(0, out subscriberCollection);

            return result;
        }

        public bool TryGetSubscriptions(ExKeyT exKey, out SubscriberCollection subscriberCollection)
        {
            bool result = _propIndexes.TryGetSubscriberCollection(exKey.Level2Key, out subscriberCollection);

            return result;
        }

        private SubscriberCollection GetOrAddSubscriberCollection(ExKeyT exKey)
        {
            SubscriberCollection result = _propIndexes.GetOrCreate(exKey.Level2Key, out bool subcriberListWasCreated);
            if (subcriberListWasCreated)
            {
                System.Diagnostics.Debug.WriteLine($"Created a new SubscriberCollection for {exKey}.");
            }

            return result;
        }

        #endregion

        #region IRegisterBindings Implementation

        public bool RegisterBinding<T>(IPropBag targetPropBag, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            ExKeyT exKey = GetExKey(targetPropBag, propId);
            return RegisterBinding<T>(exKey, bindingInfo);
        }

        private bool RegisterBinding<T>(ExKeyT propId, LocalBindingInfo bindingInfo)
        {
            ISubscriptionKeyGen BindingRequest = new BindingSubscriptionKey<T>(propId, bindingInfo);
            ISubscription newSubscription = AddBinding(BindingRequest, out bool wasAdded);
            return wasAdded;
        }

        public bool UnregisterBinding<T>(IPropBag targetPropBag, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            ExKeyT exKey = GetExKey(targetPropBag, propId);
            return UnregisterBinding<T>(exKey, bindingInfo);
        }

        public bool UnregisterBinding<T>(ExKeyT propId, LocalBindingInfo bindingInfo)
        {
            ISubscriptionKeyGen bindingRequest = new BindingSubscriptionKey<T>(propId, bindingInfo);

            bool result = TryRemoveBinding(bindingRequest, out ISubscription binding);
            if (binding is IDisposable disable)
            {
                disable.Dispose();
            }
            return result;
        }

        //public bool RegisterBinding<T>(IPropBag targetPropBag, PropIdType propId, LocalBindingInfo bindingInfo)
        //{
        //    bool result = RegisterBinding(targetPropBag, propId, bindingInfo);
        //    return result;
        //}

        //public bool RegisterBinding(IPropBag targetPropBag, PropIdType propId, LocalBindingInfo bindingInfo)
        //{
        //    ExKeyT exKey = GetExKey(targetPropBag, propId);
        //    ISubscriptionKeyGen BindingRequest = new BindingSubscriptionKey<T>(exKey, bindingInfo);
        //    ISubscription newSubscription = AddBinding(BindingRequest, out bool wasAdded);
        //    return wasAdded;
        //}

        //public bool UnRegisterBinding<T>(IPropBag targetPropBag, PropIdType propId, LocalBindingInfo bindingInfo)
        //{
        //    bool result = UnRegisterBinding(targetPropBag, propId, bindingInfo);
        //    return result;
        //}

        //public bool UnRegisterBinding(IPropBag targetPropBag, PropIdType propId, LocalBindingInfo bindingInfo)
        //{
        //    ExKeyT exKey = GetExKey(targetPropBag, propId);
        //    ISubscriptionKeyGen bindingRequest = new BindingSubscriptionKey<T>(exKey, bindingInfo);

        //    bool result = TryRemoveBinding(bindingRequest, out ISubscription binding);
        //    if (binding is IDisposable disable)
        //    {
        //        disable.Dispose();
        //    }
        //    return result;
        //}

        #endregion

        #region Binding Management

        public ISubscription AddBinding(ISubscriptionKeyGen bindingRequest, out bool wasAdded)
        {
            if (bindingRequest.HasBeenUsed)
            {
                throw new ApplicationException("Its already been used.");
            }

            ISubscription result = _bindings.GetOrAdd(bindingRequest, x => bindingRequest.CreateBinding(this));

            if (bindingRequest.HasBeenUsed)
            {
                System.Diagnostics.Debug.WriteLine($"Created a new Binding for Property:" +
                    $" {bindingRequest.OwnerPropId} / Event: {result.SubscriptionKind}.");
                wasAdded = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"The Binding for Property:" +
                    $" {bindingRequest.OwnerPropId} / Event: {result.SubscriptionKind} was not added.");
                wasAdded = false;
            }

            return result;

        }

        public bool TryRemoveBinding(ISubscriptionKeyGen bindingRequest, out ISubscription binding)
        {
            bool result = _bindings.TryRemoveBinding(bindingRequest, out binding);
            return result;
        }

        //public bool TryRemoveBinding(IPropBag host, uint propId)
        //{
        //    ExKeyT exKey = GetExKey(host, propId);

        //    bool wasRemoved = _bindings.TryRemoveBinding(exKey);
        //    return wasRemoved;
        //}

        public IEnumerable<ISubscription> GetBindings(IPropBag host, uint propId)
        {
            ExKeyT exKey = GetExKey(host, propId);

            IEnumerable<ISubscription> result = _bindings.TryGetBindings(exKey);
            return result;                
        }

        #endregion

        #region Prop Store Node Events and Call Backs

        private bool BeginWatchingParent(ExKeyT propertyKey)
        {
            //TryGetSubscriptions(propertyKey, out SubscriberCollection sc);

            SubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyKey,
                OurParentHasChanged, SubscriptionPriorityGroup.Internal, keepRef: false);

            AddSubscription(subscriptionRequest, out bool wasAdded);

            //TryGetSubscriptions(propertyKey, out SubscriberCollection sc2);
            return wasAdded;
        }

        private bool StopWatchingParent(ExKeyT propertyKey)
        {
            SubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyKey, 
                OurParentHasChanged, SubscriptionPriorityGroup.Internal, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        // Our call back to let us know to update the parentage of a IPropBag object.
        private void OurParentHasChanged(object sender, PcObjectEventArgs e)
        {
            if (e.NewValueIsUndefined)
            {
                if (e.OldValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.OldValue.GetType().IsPropBagBased(), "The old value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Make old a root.
                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal((IPropBag)e.OldValue);
                    guestPropBagNode.Parent = null;
                }
            }
            else if (e.OldValueIsUndefined)
            {
                if (e.NewValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.NewValue.GetType().IsPropBagBased(), "The new value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Move to child of our property item. This object is currently a root.
                    IPropBag propBagHost = (IPropBag)e.NewValue;

                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal(propBagHost);

                    L2KeyManType level2Man = ((IPropBagInternal)sender).ItsStoreAccessor.Level2KeyManager;
                    if (level2Man.TryGetFromRaw(e.PropertyName, out PropIdType propId))
                    {
                        ExKeyT cKey = new SimpleExKey(_objectId, propId);
                        StoreNodeProp propItemNode = GetChild(cKey);
                        guestPropBagNode.Parent = propItemNode;
                    }
                }
            }
            else
            {
                // Out with the old, and in with the new.
                if (e.OldValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.OldValue.GetType().IsPropBagBased(), "The old value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Make old a root.
                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal((IPropBag)e.OldValue);
                    guestPropBagNode.Parent = null;
                }

                if (e.NewValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.NewValue.GetType().IsPropBagBased(), "The new value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Move to child of our property item. This object is currently a root.
                    IPropBag propBagHost = (IPropBag)e.NewValue;

                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal(propBagHost);

                    L2KeyManType level2Man = ((IPropBagInternal)sender).ItsStoreAccessor.Level2KeyManager;
                    if (level2Man.TryGetFromRaw(e.PropertyName, out PropIdType propId))
                    {
                        ExKeyT cKey = new SimpleExKey(_objectId, propId);
                        StoreNodeProp propItemNode = GetChild(cKey);

                        // Update, if not already set.
                        if (guestPropBagNode.Parent != propItemNode)
                        {
                            guestPropBagNode.Parent = propItemNode;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("The PropItemNode is already our parent.");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not find property with name: {e.PropertyName} from {e.NewValue}.");
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private StoreNodeBag GetGuestObjectNodeFromPropItemVal(IPropBag propBag)
        {
            //ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            if (propBag is IPropBagInternal pbInternalAccess)
            {
                PSAccessServiceType accessService = pbInternalAccess.ItsStoreAccessor;
                if (accessService is IHaveTheStoreNode itsGotTheKey)
                {
                    StoreNodeBag propStoreNode = itsGotTheKey.PropStoreNode;
                    return propStoreNode;
                }
                else
                {
                    throw new InvalidOperationException($"The {nameof(propBag)}'s {nameof(pbInternalAccess.ItsStoreAccessor)} does not implement the {nameof(IHaveTheStoreNode)} interface.");
                }
            }
            else
            {
                throw new ArgumentException($"{nameof(propBag)} does not implement the {nameof(IPropBagInternal)} interface.", nameof(propBag));
            }
        }

        StoreNodeBag _root;
        private StoreNodeBag Root
        {
            get
            {
                if(_root == null)
                {
                    _root = _ourNode.Root;
                }
                return _root;
            }
        }

        private StoreNodeProp GetChild(ExKeyT cKey)
        {
            if (_ourNode.TryGetChild(cKey, out StoreNodeProp child))
            {
                StoreNodeProp result = child;
                return result;
            }
            else
            {
                throw new KeyNotFoundException("That propId could not be found.");
            }
        }

        private StoreNodeProp GetChild(PropIdType propId)
        {
            ExKeyT cKey = new SimpleExKey(this._objectId, propId);
            if (_ourNode.TryGetChild(cKey, out StoreNodeProp child))
            {
                StoreNodeProp result = child;
                return result;
            }
            else
            {
                throw new KeyNotFoundException("That propId could not be found.");
            }
        }

        private StoreNodeProp CheckAndGetChild(IPropBag propBag, PropIdType propId, out ExKeyT cKey)
        {
            cKey = GetCompKey(propBag, propId);
            StoreNodeProp result = GetChild(cKey);
            return result;
        }

        //private IPropBagProxy GetInt_PropBag(IPropBag propBagWithInt)
        //{
        //    if (propBagWithInt is IPropBagProxy ipbi)
        //    {
        //        return ipbi;
        //    }
        //    else
        //    {
        //        throw new ArgumentException($"int_PropBag does not implement {nameof(IPropBagProxy)}.");
        //    }
        //}

        //private IPropDataInternal GetInt_PropData(IPropData propDataWithInt)
        //{
        //    if (propDataWithInt is IPropDataInternal int_PropData)
        //    {
        //        return int_PropData;
        //    }
        //    else
        //    {
        //        throw new ArgumentException($"propDataWithInt does not implement {nameof(propDataWithInt)}.");
        //    }
        //}

        ExKeyT GetCompKey(IPropBag propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            ExKeyT cKey = new SimpleExKey(objectId, propId);

            return cKey;
        }

        private string GetPropNameFromKey(ExKeyT cKey)
        {
            if(Level2KeyManager.TryGetFromCooked(cKey.Level2Key, out PropNameType propertyName))
            {
                return propertyName;
            }
            else
            {
                throw new KeyNotFoundException($"The cKey: {cKey}, which includes PropId: {cKey.Level2Key} does not correspond with any registered propertyName.");
            }
        }

        private ExKeyT GetExKey(IPropBag propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            ExKeyT exKey = new SimpleExKey(objectId, propId);
            return exKey;
        }

        private ObjectIdType GetAndCheckObjectRef(IPropBag propBag)
        {
            if(_clientAccessToken.TryGetTarget(out IPropBagInternal target))
            {
                if (!object.ReferenceEquals(propBag, target))
                {
                    throw new InvalidOperationException("This PropStoreAccessService can only service the PropBag object that created it.");
                }

                ObjectIdType result = _objectId;
                return result;
            }
            else
            {
                throw new InvalidOperationException("The weak reference to the PropBag held by the StoreAccessor, holds no object.");
            }
        }

        #endregion

        #region Explicit Implementation of the internal interface: IHaveTheKey

        StoreNodeBag IHaveTheStoreNode.PropStoreNode => _ourNode;

        #endregion

        #region IDisposable Support


        private void ResetAllData()
        {
            // Remove all handlers from each of our Events.
            int numSubsRemoved = 0;
            foreach (SubscriberCollection sc in _propIndexes)
            {
                numSubsRemoved += sc.ClearSubscriptions();
            }

            // Clear the contents of our list of Handlers, one for each property.
            _propIndexes.ClearTheListOfSubscriptionPtrs();

            // Dispose of each LocalBinding object.
            foreach (ISubscription binding in _bindings)
            {
                if (binding is IDisposable disable) disable.Dispose();
            }

            // Remove our reference to each LocalBinding.
            int numBindingsRemoved = _bindings.ClearBindings();

            // Dispose each PropItem.
            foreach (StoreNodeProp prop in _ourNode.Children)
            {
                prop.Int_PropData.CleanUp(doTypedCleanup: true);
            }

            _propStoreAccessServiceProvider.TearDown(_ourNode.CompKey);

            Level2KeyManager.Dispose();
            Level2KeyManager = null;
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    ResetAllData();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

        #region Diagnostics

        public void IncAccess()
        {
            _propStoreAccessServiceProvider.IncAccess();
        }

        public PSAccessServiceType CloneProps(IPropBag callingPropBag, IPropBag copySource)
        {
            if(!(copySource is IPropBagInternal int_propBag))
            {
                throw new InvalidOperationException($"The {nameof(copySource)} does not implement the {nameof(IPropBagInternal)} interface.");
            }

            if (!(callingPropBag is IPropBagInternal target))
            {
                throw new InvalidOperationException($"The {nameof(target)} does not implement the {nameof(IPropBagInternal)} interface.");
            }

            GetAndCheckObjectRef(callingPropBag);

            PSCloneServiceType accessorCloneService = (PSCloneServiceType)_propStoreAccessServiceProvider;

            PSAccessServiceType newStoreAccessor = accessorCloneService.CloneService
                (
                    int_propBag,
                    int_propBag.ItsStoreAccessor,
                    target,
                    out StoreNodeBag copySourceStoreNode,
                    out StoreNodeBag newStoreNode
                );

            System.Diagnostics.Debug.Assert(
                condition: newStoreAccessor.Level2KeyManager.PropertyCount == ((IPropBagInternal)copySource).ItsStoreAccessor.Level2KeyManager.PropertyCount,
                message: "The PropBag clone operation was not completed: The Level2KeyManager has different contents."
                );


            CopyChildProps(copySourceStoreNode, newStoreNode);

            return newStoreAccessor;
        }

        // TODO: What about the subscriptions and bindings that were included in the PropModel that were used to create these PropItems?
        private void CopyChildProps(StoreNodeBag sourceBag, StoreNodeBag newBagNode)
        {
            foreach (StoreNodeProp childProp in sourceBag.Children)
            {
                ExKeyT newCKey = new SimpleExKey(newBagNode.ObjectId, childProp.PropId);

                // FOR DEBUGGING -- To see how well the PropBag value is cloned.
                if (childProp.Int_PropData.TypedProp.Type.IsPropBagBased())
                {
                    if (childProp.Int_PropData.TypedProp.TypedValueAsObject != null)
                    {
                        IPropBagInternal propBagInternal = (IPropBagInternal)childProp.Int_PropData.TypedProp.TypedValueAsObject;
                    }
                }

                IPropDataInternal newPropGen = new PropGen(newCKey, (IProp)childProp.Int_PropData.TypedProp.Clone());

                StoreNodeProp newChild = new StoreNodeProp(newCKey, newPropGen, newBagNode);
            }
        }

        public int AccessCounter => _propStoreAccessServiceProvider.AccessCounter;

        #endregion
    }
}
