using DRM.TypeSafePropertyBag.DataAccessSupport;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeSafePropertyBag.LocalBinding;
using ObjectSizeDiagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;
    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;

    internal class SimplePropStoreAccessService : PSAccessServiceInterface, IHaveTheStoreNode, PSAccessServiceInternalInterface, IDisposable
    {
        #region Private Members

        readonly WeakReference<PSAccessServiceInterface> _wrThis;
        readonly WeakRefKey<IPropBag> _clientAccessToken;
        readonly ObjectIdType _objectId;

        readonly StoreNodeBag _ourNode;

        PSAccessServiceProviderInterface _propStoreAccessServiceProvider;
        readonly IProvideHandlerDispatchDelegateCaches _handlerDispatchDelegateCacheProvider;
        //L2KeyManType _level2KeyMan;

        //// Subscription Management
        //const int OBJECT_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        //const int EXPECTED_NO_OF_OBJECTS = 10000;

        // Each PropItem has a collection of Subscribers for a variety of event types.
        private CollectionOfSubscriberCollections _propIndexes;

        // Each PropItem has zero or one binding.
        private BindingsCollection _bindings;

        // One item for each PropItem that hosts a data source from which a view can be built.
        // The data source and the view manager are both local to this clients IPropBag.
        private ViewManagerCollection _genViewManagers;

        // One item for each PropItem that binds to a Collection View Manager hosted by a 'foreign' (reached with a relative binding path) IPropBag.
        private ViewManagerProviderCollection _genViewManagerProviders;

        #endregion

        #region Constructor

        public SimplePropStoreAccessService
            (
            StoreNodeBag ourNode,
            //L2KeyManType level2KeyManager,
            PSAccessServiceProviderInterface propStoreAccessServiceProvider,
            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider
            )
        {
            //long startBytes = System.GC.GetTotalMemory(true);
            //long CURbytes = startBytes;

            _wrThis = new WeakReference<PSAccessServiceInterface>(this);
            _ourNode = ourNode;
            _propStoreAccessServiceProvider = propStoreAccessServiceProvider;
            _handlerDispatchDelegateCacheProvider = handlerDispatchDelegateCacheProvider;

            MaxObjectsPerAppDomain = propStoreAccessServiceProvider.MaxObjectsPerAppDomain;

            _clientAccessToken = _ourNode.PropBagProxy;
            _objectId = _ourNode.CompKey.Level1Key;

            //CURbytes = Sizer.ReportMemConsumption(startBytes, CURbytes, "Before Create PropIndexes");

            // Create the subscription store for this PropBag.
            _propIndexes = null; // new CollectionOfSubscriberCollections();
            //CURbytes = Sizer.ReportMemConsumption(startBytes, CURbytes, "After Create PropIndexes");


            // Create the binding store for this PropBag.
            _bindings = null; // new BindingsCollection();
            //CURbytes = Sizer.ReportMemConsumption(startBytes, CURbytes, "After Create BindingsCollection");

            _genViewManagers = null;
            _genViewManagerProviders = null;
        }

        #endregion

        #region Public Members

        public int MaxPropsPerObject => _ourNode.Level2KeyMan.MaxPropsPerObject; // _level2KeyMan.MaxPropsPerObject;
        public long MaxObjectsPerAppDomain { get; }

        public IPropData this[IPropBag propBag, PropIdType propId]
        {
            get
            {
                ExKeyT cKey = GetCompKey(propBag, propId);
                IPropData result = GetChild(cKey).PropData_Internal;
                return result;
            }
        }

        public bool TryGetValue(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            ExKeyT cKey = GetCompKey(propBag, propId);
            if (_ourNode.TryGetChild(cKey, out StoreNodeProp child))
            {
                propData = child.PropData_Internal;
                return true;
            }
            else
            {
                propData = null;
                return false;
            }
        }

        // Add PropItem with no subscription.
        public bool TryAdd(IPropBag propBag, PropIdType propId, PropNameType propertyName, IProp genericTypedProp, out IPropData propData)
        {
            StoreNodeProp newNode = TryAddFirstPart(propBag, propId, genericTypedProp);
            propData = newNode.PropData_Internal;

            TryAddSecondPart(newNode, propertyName);

            return true;
        }

        // Add PropItem with PcTyped subscription
        public bool TryAdd<PropT>(IPropBag propBag, PropIdType propId, PropNameType propertyName, IProp genericTypedProp,
            EventHandler<PcTypedEventArgs<PropT>> eventHandler, SubscriptionPriorityGroup priorityGroup, out IPropData propData)
        {
            StoreNodeProp newNode = TryAddFirstPart(propBag, propId, genericTypedProp);
            propData = newNode.PropData_Internal;

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

            TryAddSecondPart(newNode, propertyName);

            return result;
        }

        // Add PropItem with Target/Method subscription
        public bool TryAdd(IPropBag propBag, PropIdType propId, PropNameType propertyName, IProp genericTypedProp,
            object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, out IPropData propData)
        {
            StoreNodeProp newNode = TryAddFirstPart(propBag, propId, genericTypedProp);
            propData = newNode.PropData_Internal;

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

            TryAddSecondPart(newNode, propertyName);

            return result;
        }

        // Add PropItem with PcGen subscription
        public bool TryAdd(IPropBag propBag, PropIdType propId, PropNameType propertyName, IProp genericTypedProp,
            EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, out IPropData propData)
        {
            StoreNodeProp newNode = TryAddFirstPart(propBag, propId, genericTypedProp);
            propData = newNode.PropData_Internal;

            bool result;
            if (eventHandler != null)
            {
                IDisposable disable = RegisterHandler(newNode.CompKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
                result = disable != null;
            }
            else
            {
                result = true;
            }

            TryAddSecondPart(newNode, propertyName);

            return result;
        }

        private StoreNodeProp TryAddFirstPart(IPropBag propBag, PropIdType propId, IProp genericTypedProp)
        {
            IPropDataInternal propData_Internal = new PropGen(genericTypedProp);

            ExKeyT propertyKey = GetCompKey(propBag, propId);
            StoreNodeProp propStoreNode = new StoreNodeProp(propertyKey, propData_Internal, _ourNode);

            return propStoreNode;
        }

        private void TryAddSecondPart(StoreNodeProp propStoreNode, PropNameType propertyName)
        {
            IPropDataInternal propData = propStoreNode.PropData_Internal;

            if (propData.IsPropBag)
            {
                // If the new property is of a type that implements IPropBag,
                // attempt to get a reference to the StoreAccessor of that IPropBag object,
                // and from that StoreAccessor, the object id of the PropBag.
                // Use the PropBag's ObjectId to set the ChildObjectId of this new property.

                object guest = propData?.TypedProp?.TypedValueAsObject;
                if (guest != null)
                {
                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal(guest);
                    guestPropBagNode.Parent =  propStoreNode;
                }

                // Subscribe to changes to this PropData's Value.
                BeginWatchingParent(propStoreNode.CompKey);
            }

            if(propData.TypedProp.StorageStrategy == PropStorageStrategyEnum.Virtual)
            {
                EventHandler<EventArgs> x = CreateVirtualValueChangedHandler(propStoreNode, propertyName);
                if (x != null)
                {
                    propData.TypedProp.ValueChanged += x;
                }
            }
        }

        private EventHandler<EventArgs> CreateVirtualValueChangedHandler(StoreNodeProp propNode, PropNameType propertyName)
        {
            StoreNodeBag hostBagNode = propNode?.Parent;

            if (hostBagNode == null) return null;

            WeakRefKey<IPropBag> propBagRef = hostBagNode.PropBagProxy;
            PropIdType propId = propNode.PropId;
            //PropNameType propertyName = GetPropNameFromKey(propNode.CompKey.Level2Key);

            return newHandler;

            void newHandler(object sender, EventArgs e)
            {
                this.ForwardRequestToRaiseStandardPC(propBagRef, propId, propertyName);
            }
        }

        private void ForwardRequestToRaiseStandardPC(WeakRefKey<IPropBag> propBagRef, PropIdType propId, PropNameType propName)
        {
            if (propBagRef.TryGetTarget(out IPropBag propBag))
            {
                propBag.RaiseStandardPropertyChanged(/*propId, */propName);
            }
        }

        public bool TryRemove(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            ExKeyT cKey = GetCompKey(propBag, propId);

            bool result = _ourNode.TryRemoveChild(cKey, out StoreNodeProp child);

            if (result)
                propData = child.PropData_Internal;
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
                x => GetPropNameFromKey(x.CompKey.Level2Key),
                x => (IPropData)x.PropData_Internal
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
            IEnumerable<PropNameType> result = _ourNode.Children.Select(x => GetPropNameFromKey(x.CompKey.Level2Key));
            return result;
        }

        public IEnumerable<IPropData> GetValues(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);
            IEnumerable<IPropData> result = _ourNode.Children.Select(x => x.PropData_Internal);
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

        public bool TryGetPropId(PropNameType propertyName, out PropIdType propId)
        {
            return _ourNode.Level2KeyMan.TryGetFromRaw(propertyName, out propId);
        }

        public bool TryGetPropName(PropIdType propId, out PropNameType propertyName)
        {
            return _ourNode.Level2KeyMan.TryGetFromCooked(propId, out propertyName);
        }

        public uint Add(string propertyName)
        {
            return _ourNode.Level2KeyMan.Add(propertyName);
        }

        public int PropertyCount => _ourNode.Level2KeyMan.PropertyCount;

        public PSAccessServiceInterface CloneProps(IPropBag callingPropBag, IPropBag copySource)
        {
            // Since the caller does not yet have a StoreAccessor (this method is responsble for creating the new StoreAccessor),
            // the caller is using the StoreAccessor that belongs to the copySource to make this call.
            GetAndCheckObjectRef(copySource);

            StoreNodeBag copySourceStoreNode = GetPropBagNode(this);

            L2KeyManType sourceLevel2KeyMan = _ourNode.Level2KeyMan;

            PSAccessServiceInterface newStoreAccessor = _propStoreAccessServiceProvider.ClonePSAccessService
                (
                    copySource,
                    this,
                    sourceLevel2KeyMan,
                    callingPropBag, // The PropBag for which the newStoreAcessor will be built.
                    out StoreNodeBag newStoreNode
                );

            System.Diagnostics.Debug.Assert
                (
                condition: ((IHaveTheStoreNode)newStoreAccessor).PropBagNode.Level2KeyMan.PropertyCount == sourceLevel2KeyMan.PropertyCount,
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
                //// FOR DEBUGGING -- To see how well the PropBag value is cloned.
                //if (childProp.PropData_Internal.TypedProp.Type.IsPropBagBased())
                //{
                //    if (childProp.PropData_Internal.TypedProp.TypedValueAsObject != null)
                //    {
                //        IPropBagInternal propBagInternal = (IPropBagInternal)childProp.PropData_Internal.TypedProp.TypedValueAsObject;
                //    }
                //}

                IPropDataInternal newPropGen = new PropGen((IProp)childProp.PropData_Internal.TypedProp.Clone());
                ExKeyT newCKey = new SimpleExKey(newBagNode.ObjectId, childProp.PropId);
                StoreNodeProp newChild = new StoreNodeProp(newCKey, newPropGen, newBagNode);
            }
        }

        #endregion

        #region IRegisterSubscriptions Implementation

        #region PC Typed Event Args

        public IDisposable RegisterHandler<T>(IPropBag propBag, PropIdType propId,
            EventHandler<PcTypedEventArgs<T>> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetCompKey(propBag, propId);

            IDisposable disable = RegisterHandler<T>(exKey, eventHandler, priorityGroup, keepRef);
            return disable;
        }

        private IDisposable RegisterHandler<T>(ExKeyT propertyId, EventHandler<PcTypedEventArgs<T>> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKey<T>(propertyId, eventHandler, priorityGroup, keepRef);
            ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);

            Unsubscriber disable = new Unsubscriber(_wrThis, subscriptionRequest);
            return disable;
        }

        public bool UnregisterHandler<T>(IPropBag propBag, PropIdType propId, EventHandler<PcTypedEventArgs<T>> eventHandler)
        {
            ExKeyT exKey = GetCompKey(propBag, propId);

            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKey<T>(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        private bool UnregisterHandler<T>(ExKeyT exKey, EventHandler<PcTypedEventArgs<T>> eventHandler)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKey<T>(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region PC Gen Event Args

        public IDisposable RegisterHandler(IPropBag propBag, uint propId,
            EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT propertyId = GetCompKey(propBag, propId);
            IDisposable result = RegisterHandler(propertyId, eventHandler, priorityGroup, keepRef);
            return result;
        }

        private IDisposable RegisterHandler(ExKeyT propertyId, EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyId, eventHandler, priorityGroup, keepRef);
            ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);

            Unsubscriber disable = new Unsubscriber(_wrThis, subscriptionRequest);
            return disable;
        }

        public bool UnregisterHandler(IPropBag propBag, uint propId, EventHandler<PcGenEventArgs> eventHandler)
        {
            ExKeyT exKey = GetCompKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        private bool UnregisterHandler(ExKeyT exKey, EventHandler<PcGenEventArgs> eventHandler)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region PC Object Event Args

        public bool RegisterHandler(IPropBag propBag, uint propId,
            EventHandler<PcObjectEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetCompKey(propBag, propId);
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
            ExKeyT exKey = GetCompKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region PC Standard Event Args

        public bool RegisterHandler(IPropBag propBag, uint propId,
            PropertyChangedEventHandler eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetCompKey(propBag, propId);
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
            ExKeyT exKey = GetCompKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region PC Changing Event Args

        public bool RegisterHandler(IPropBag propBag, uint propId,
            PropertyChangingEventHandler eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetCompKey(propBag, propId);
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
            ExKeyT exKey = GetCompKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region Target and Method

        public bool RegisterHandler(IPropBag propBag, uint propId, Type propertyType, object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetCompKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, propertyType, target, method, subscriptionKind, priorityGroup, keepRef, subscriptionFactory: null);

            ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);
            return wasAdded;
        }

        public bool UnregisterHandler(IPropBag propBag, uint propId, Type propertyType, object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetCompKey(propBag, propId);
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

        // TODO: Consider making this, or adding a method: TryRemoveSubscription
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
            // TODO: NOTE: This does not verify that the caller is the "correct" one.
            ExKeyT exKey = new SimpleExKey(_objectId, propId);

            IEnumerable<ISubscription> result;
            if (TryGetSubscriptions(exKey, out IEnumerable<ISubscription> subs))
            {
                result = subs;
            }
            else
            {
                result = new SubscriberCollection();
            }

            return result;
        }

        public bool TryGetSubscriptions(ExKeyT exKey, out IEnumerable<ISubscription> subscriberCollection)
        {
            bool result = TryGetSubscriptions(exKey, out SubscriberCollection subs);
            subscriberCollection = subs;
            return result;
        }

        public bool TryGetSubscriptions(ExKeyT exKey, out SubscriberCollection subscriberCollection)
        {
            if (_propIndexes != null)
            {
                bool result = _propIndexes.TryGetSubscriberCollection(exKey.Level2Key, out subscriberCollection);
                return result;
            }
            else
            {
                subscriberCollection = new SubscriberCollection();
                return false;
            }
        }

        private SubscriberCollection GetOrAddSubscriberCollection(ExKeyT exKey)
        {
            if(_propIndexes == null)
            {
                _propIndexes = new CollectionOfSubscriberCollections();
            }

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
            ExKeyT exKey = GetCompKey(targetPropBag, propId);
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
            ExKeyT exKey = GetCompKey(targetPropBag, propId);
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
        //    ExKeyT exKey = GetCompKey(targetPropBag, propId);
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
        //    ExKeyT exKey = GetCompKey(targetPropBag, propId);
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

            if(_bindings == null)
            {
                _bindings = new BindingsCollection();
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
            bool result;

            if (_bindings != null)
            {
                result = _bindings.TryRemoveBinding(bindingRequest, out binding);
            }
            else
            {
                binding = null;
                result = false;
            }

            return result;
        }

        public IEnumerable<ISubscription> GetBindings(IPropBag host, PropIdType propId)
        {
            IEnumerable<ISubscription> result;

            if (_bindings != null)
            {
                ExKeyT exKey = GetCompKey(host, propId);
                result = _bindings.TryGetBindings(exKey);
            }
            else
            {
                result = Enumerable.Empty<ISubscription>();
            }

            return result;
        }



        #endregion

        #region ViewManger and Data Source Provider Support

        // Provides thread-safe, lazy production of a single DataSourceProvider for each PropItem.

        // Get a DataSourceProvider
        public DataSourceProvider GetOrAddDataSourceProvider(IPropBag propBag, PropIdType propId, IPropData propData, CViewProviderCreator viewBuilder)
        {
            IManageCViews CViewManagerGen = GetOrAddViewManager(propBag, propId, propData, viewBuilder);
            DataSourceProvider result = CViewManagerGen.DataSourceProvider;
            return result;
        }

        public DataSourceProvider GetDataSourceProvider(IPropBag propBag, PropIdType propId)
        {
            IManageCViews cViewManager = GetViewManager(propBag, propId);

            if (cViewManager == null) return null;

            return cViewManager.DataSourceProvider;
        }

        public IManageCViews GetViewManager(IPropBag propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            // There is one View Manager for each PropItem. The View Manager for a particular PropItem is created on first use.
            if (_genViewManagers == null)
            {
                return null;
            }
            else
            {
                if (_genViewManagers.TryGetValue(propId, out IManageCViews cViewManager))
                {
                    IManageCViews result = cViewManager;
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        // ViewManager from ObservableCollection
        // Build a ViewManager whose source is a PropItem of Kind = ObservableCollection
        // A DataSourceProvider, a CollectionViewSource and a ListCollectionView are created.
        // The DataSourceProvider not only raises the standard DataChanged event, but also raises
        // EventHandler<EventArgs> ItemEndEdit events whenever an item in the list raises it's ItemEndEdit event.
        public IManageCViews GetOrAddViewManager
        (
            IPropBag propBag,
            PropIdType propId,
            IPropData propData,
            CViewProviderCreator viewBuilder
        )
        {
            ObjectIdType objectId  = GetAndCheckObjectRef(propBag);

            // There is one View Manager for each PropItem. The View Manager for a particular PropItem is created on first use.
            if (_genViewManagers == null)
                _genViewManagers = new ViewManagerCollection();

            IManageCViews result = _genViewManagers.GetOrAdd(propId, CViewGenManagerFactory);
            return result;

            IManageCViews CViewGenManagerFactory(PropIdType propId2)
            {
                
                IWatchAPropItemGen propItemWatcherGen = new PropItemWatcherGen(this, propId2);

                IProvideADataSourceProvider dSProviderProvider = new PBCollectionDSP_Provider(propData.TypedProp.PropKind, propItemWatcherGen);
                IManageCViews result2 = new ViewManager(dSProviderProvider, viewBuilder);
                return result2;
            }
        }

        // ViewManager from DataSourceProvider-Provider
        public IManageCViews GetOrAddViewManager
        (
            IPropBag propBag,
            PropIdType propId,
            IPropData propData,
            CViewProviderCreator viewBuilder,
            IProvideADataSourceProvider dSProviderProvider
        )
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            // There is one View Manager for each PropItem. The View Manager for a particular PropItem is created on first use.
            if (_genViewManagers == null)
                _genViewManagers = new ViewManagerCollection();

            IManageCViews result = _genViewManagers.GetOrAdd(propId, CViewGenManagerFactory);
            return result;

            IManageCViews CViewGenManagerFactory(PropIdType propId2)
            {
                IManageCViews result2 = new ViewManager(dSProviderProvider, viewBuilder);
                return result2;
            }
        }

        // ViewManager from IDoCRUD<T>, optionally using an IMapperRequest and propBagMapper factory.     
        // Build a ViewManager whose source is a PropItem of Kind = Prop and whose type is IDoCrud<T>
        // A DataSourceProvider, a CollectionViewSource and a ListCollectionView are created.
        // The DataSourceProvider not only raises the standard DataChanged event, but also raises
        // EventHandler<EventArgs> ItemEndEdit events whenever an item in the list raises it's ItemEndEdit event.
        public IManageCViews GetOrAddViewManager<TDal, TSource, TDestination> 
        (
            IPropBag propBag,   // The client of this service.
            PropIdType propId,  // Identifies the PropItem that implements IDoCrud<TSource>
            IPropData propData, // The PropStore management wrapper for IProp<TSource> which holds the value of the 'IDoCrud<T>' data access layer.
            IMapperRequest mr,  // The information necessary to create a IPropBagMapper<TSource, TDestination>
            PropBagMapperCreator propBagMapperCreator,  // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
            CViewProviderCreator viewBuilder            // Method that can be used to create a IProvideAView from a DataSourceProvider.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            // There is one View Manager for each PropItem. The View Manager for a particular PropItem is created on first use.
            if (_genViewManagers == null)
                _genViewManagers = new ViewManagerCollection();

            IManageCViews result = _genViewManagers.GetOrAdd(propId, CViewGenManagerFactory);
            return result;

            IManageCViews CViewGenManagerFactory(PropIdType propId2)
            {
                IProvideADataSourceProvider dSProviderProvider;
                if (propData.TypedProp.PropKind == PropKindEnum.Prop)
                {
                    IWatchAPropItem<TDal> propItemWatcher = new PropItemWatcher<TDal>(this as PSAccessServiceInternalInterface, propId2);

                    IPropBagMapper<TSource, TDestination> mapper = propBagMapperCreator(mr) as IPropBagMapper<TSource, TDestination>;

                    CrudWithMapping<TDal, TSource, TDestination> mappedDal = 
                        new CrudWithMapping<TDal, TSource, TDestination>(propItemWatcher, mapper);

                    dSProviderProvider = new ClrMappedDSP<TDestination>(mappedDal);
                }
                else
                {
                    throw new InvalidOperationException("This version of GetOrAddViewManager requires a PropItem of kind = Prop and PropertyType = IDoCRUD<T>.");
                    //dSProviderProvider = new PBCollectionDSP_Provider(propId, propData.TypedProp.PropKind, this);
                }

                IManageCViews result2 = new ViewManager(dSProviderProvider, viewBuilder);
                return result2;
            }
        }

        // ViewManager from IDoCRUD<T>, optionally using an IPropBagMapper.     
        // Build a ViewManager whose source is a PropItem of Kind = Prop and whose type is IDoCrud<T>
        // A DataSourceProvider, a CollectionViewSource and a ListCollectionView are created.
        // The DataSourceProvider not only raises the standard DataChanged event, but also raises
        // EventHandler<EventArgs> ItemEndEdit events whenever an item in the list raises it's ItemEndEdit event.
        public IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
        (
            IPropBag propBag,   // The client of this service.
            PropIdType propId,  // Identifies the PropItem that implements IDoCrud<TSource>
            IPropData propData, // The PropStore management wrapper for IProp<TSource> which holds the value of the 'IDoCrud<T>' data access layer.
            IPropBagMapper<TSource, TDestination> mapper,   // The AutoMapper used to translate from source data to view items.
            CViewProviderCreator viewBuilder                // Method that can be used to create a IProvideAView from a DataSourceProvider.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            // There is one View Manager for each PropItem. The View Manager for a particular PropItem is created on first use.
            if (_genViewManagers == null)
                _genViewManagers = new ViewManagerCollection();

            IManageCViews result = _genViewManagers.GetOrAdd(propId, CViewGenManagerFactory);
            return result;

            IManageCViews CViewGenManagerFactory(PropIdType propId2)
            {
                IProvideADataSourceProvider dSProviderProvider;
                if (propData.TypedProp.PropKind == PropKindEnum.Prop)
                {
                    IWatchAPropItem<TDal> propItemWatcher = new PropItemWatcher<TDal>(this as PSAccessServiceInternalInterface, propId2);

                    CrudWithMapping<TDal, TSource, TDestination> mappedDal = new CrudWithMapping<TDal, TSource, TDestination>(propItemWatcher, mapper);
                    dSProviderProvider = new ClrMappedDSP<TDestination>(mappedDal);
                }
                else
                {
                    throw new InvalidOperationException("This version of GetOrAddViewManager requires a PropItem of kind = Prop and PropertyType = IDoCRUD<T>.");
                    //dSProviderProvider = new PBCollectionDSP_Provider(propId2, propData.TypedProp.PropKind, this);
                }

                IManageCViews result2 = new ViewManager(dSProviderProvider, viewBuilder);
                return result2;
            }
        }

        #endregion

        #region CViewManager Provider Support

        public IProvideATypedCViewManager<EndEditWrapper<TDestination>, TDestination> GetOrAddViewManagerProviderTyped<TDal, TSource, TDestination>
        (
            IPropBag propBag,   // The client of this service.
            LocalBindingInfo bindingInfo,
            IMapperRequest mr,  // The information necessary to create a IPropBagMapper<TSource, TDestination>
            PropBagMapperCreator propBagMapperCreator,  // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
            CViewProviderCreator viewBuilder            // Method that can be used to create a IProvideAView from a DataSourceProvider.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            //ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            throw new NotImplementedException("GetOrAddViewManagerProvider has not been implemented.");
        }

        public IProvideACViewManager GetOrAddViewManagerProvider<TDal, TSource, TDestination>
        (
            IPropBag propBag,   // The client of this service.
            IViewManagerProviderKey viewManagerProviderKey,
            //LocalBindingInfo bindingInfo, 
            //IMapperRequest mr,  // The information necessary to create a IPropBagMapper<TSource, TDestination>
            PropBagMapperCreator propBagMapperCreator,  // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
            CViewProviderCreator viewBuilder            // Method that can be used to create a IProvideAView from a DataSourceProvider.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            //string bindingPath = bindingInfo.PropertyPath.Path;

            if (_genViewManagerProviders == null)
                _genViewManagerProviders = new ViewManagerProviderCollection();

            //IViewManagerProviderKey viewManagerProviderKey = new ViewManagerProviderKey(bindingInfo, mr, propBagMapperCreator, viewBuilder);

            IProvideACViewManager result = _genViewManagerProviders.GetOrAdd(viewManagerProviderKey, CViewGenManagerProviderFactory);
            return result;

            IProvideACViewManager CViewGenManagerProviderFactory(IViewManagerProviderKey key)
            {
                CViewManagerBinder<TDal, TSource, TDestination> result2 =
                    new CViewManagerBinder<TDal, TSource, TDestination>(this, key, propBagMapperCreator, viewBuilder);

                return result2;
            }

            //IProvideACViewManager CViewGenManagerProviderFactory(IViewManagerProviderKey viewManagerProviderKey)
            //{
            //    CViewManagerBinder<TDal, TSource, TDestination> result2 = 
            //        new CViewManagerBinder<TDal, TSource, TDestination>(this, bindingInfo, mr, propBagMapperCreator, viewBuilder);

            //    return result2;
            //}
        }

        public bool TryGetViewManagerProvider
        (
            IPropBag propBag,   // The client of this service.
            IViewManagerProviderKey viewManagerProviderKey,
            out IProvideACViewManager provideACViewManager
        )
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            if(_genViewManagerProviders == null)
            {
                provideACViewManager = null;
                return false;
            }

            if(_genViewManagerProviders.TryGetValue(viewManagerProviderKey, out provideACViewManager))
            {
                return true;
            }
            else
            {
                provideACViewManager = null;
                return false;
            }
        }


        private bool IsBindingSourceLocal(LocalBindingInfo bindingInfo)
        {
            BindingPathParser pathParser = new BindingPathParser();
            string[] pathElements = pathParser.GetPathElements(bindingInfo, out bool isPathAbsolute, out int firstNamedStepIndex);

            bool result = pathElements.Length == 1;

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
                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal(e.OldValue);
                    guestPropBagNode.Parent = null;
                }
            }
            else if (e.OldValueIsUndefined)
            {
                if (e.NewValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.NewValue.GetType().IsPropBagBased(), "The new value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Move to child of our property item. This object is currently a root.
                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal(e.NewValue);

                    // TODO: IPBI -- Get Store Accessor From StoreNodeBag
                    //PSAccessServiceInternalInterface storeAccessor_Internal = (PSAccessServiceInternalInterface)((IPropBagInternal)sender).ItsStoreAccessor;

                    PSAccessServiceInternalInterface storeAccessor_Internal = this;

                    if (storeAccessor_Internal.TryGetChildPropNode(guestPropBagNode, e.PropertyName, out StoreNodeProp propItemNode))
                    {
                        guestPropBagNode.Parent = propItemNode;

                    }

                    //storeAccessor_Internal.TryGetChildPropNode()

                    //L2KeyManType level2Man = ((PSAccessServiceInternalInterface) ((IPropBagInternal)sender).ItsStoreAccessor).Level2KeyManager;
                    //if (level2Man.TryGetFromRaw(e.PropertyName, out PropIdType propId))
                    //{
                    //    ExKeyT cKey = new SimpleExKey(_objectId, propId);
                    //    StoreNodeProp propItemNode = GetChild(cKey);
                    //    guestPropBagNode.Parent = propItemNode;
                    //}
                }
            }
            else
            {
                // Out with the old, and in with the new.
                if (e.OldValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.OldValue.GetType().IsPropBagBased(), "The old value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Make old a root.
                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal(e.OldValue);
                    guestPropBagNode.Parent = null;
                }

                if (e.NewValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.NewValue.GetType().IsPropBagBased(), "The new value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Move to child of our property item. This object is currently a root.
                    StoreNodeBag guestPropBagNode = GetGuestObjectNodeFromPropItemVal(e.NewValue);

                    // TODO: IPBI -- Get Store Accessor From StoreNodeBag
                    //PSAccessServiceInternalInterface storeAccessor_Internal = (PSAccessServiceInternalInterface)((IPropBagInternal)sender).ItsStoreAccessor;

                    PSAccessServiceInternalInterface storeAccessor_Internal = this;


                    if (storeAccessor_Internal.TryGetChildPropNode(guestPropBagNode, e.PropertyName, out StoreNodeProp propItemNode))
                    {
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


                    //L2KeyManType level2Man = ((PSAccessServiceInternalInterface)((IPropBagInternal)sender).ItsStoreAccessor).Level2KeyManager;
                    //if (level2Man.TryGetFromRaw(e.PropertyName, out PropIdType propId))
                    //{
                    //    ExKeyT cKey = new SimpleExKey(_objectId, propId);
                    //    StoreNodeProp propItemNode = GetChild(cKey);

                    //    // Update, if not already set.
                    //    if (guestPropBagNode.Parent != propItemNode)
                    //    {
                    //        guestPropBagNode.Parent = propItemNode;
                    //    }
                    //    else
                    //    {
                    //        System.Diagnostics.Debug.WriteLine("The PropItemNode is already our parent.");
                    //    }
                    //}
                    //else
                    //{
                    //    System.Diagnostics.Debug.WriteLine($"Could not find property with name: {e.PropertyName} from {e.NewValue}.");
                    //}
                }
            }
        }

        #endregion

        #region Private Methods

        private StoreNodeBag GetGuestObjectNodeFromPropItemVal(object propItemValue)
        {
            //// TODO: IPBI -- GetStoreNode
            //if (propItemValue is IPropBagInternal pbInternalAccess)
            //{
            //    PSAccessServiceInterface accessService = pbInternalAccess.ItsStoreAccessor;

            //    StoreNodeBag result = GetPropBagNode(accessService);
            //    return result;
            //}
            //else
            //{
            //    throw new ArgumentException($"{nameof(propItemValue)} does not implement the {nameof(IPropBagInternal)} interface.", nameof(propItemValue));
            //}

            if (propItemValue is IPropBag propBag)
            {
                if(_propStoreAccessServiceProvider.TryGetPropBagNode(propBag, out StoreNodeBag propBagNode))
                {
                    return propBagNode;
                }
                else
                {
                    throw new KeyNotFoundException($"The propBag (FullClassName = {propBag.FullClassName}) could not be found in the global property store.");
                }
            }
            else
            {
                throw new ArgumentException($"{nameof(propItemValue)} does not implement the {nameof(IPropBag)} interface.", nameof(propItemValue));
            }
        }


        private StoreNodeBag GetPropBagNode(PSAccessServiceInterface propStoreAccessService)
        {
            CheckForIHaveTheStoreNode(propStoreAccessService);
            return ((IHaveTheStoreNode)propStoreAccessService).PropBagNode;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckForIHaveTheStoreNode(PSAccessServiceInterface propStoreAccessService)
        {
            if (!(propStoreAccessService is IHaveTheStoreNode storeNodeProvider))
            {
                throw new InvalidOperationException($"The {nameof(propStoreAccessService)} does not implement the {nameof(IHaveTheStoreNode)} interface.");
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


        private StoreNodeProp GetChild(PropIdType propId)
        {
            ExKeyT cKey = new SimpleExKey(this._objectId, propId);

            StoreNodeProp result = GetChild(cKey);
            return result;
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
        //    if (propDataWithInt is IPropDataInternal propData_Internal)
        //    {
        //        return propData_Internal;
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

        private string GetPropNameFromKey(PropIdType propId)
        {
            if(_ourNode.Level2KeyMan.TryGetFromCooked(propId, out PropNameType propertyName))
            {
                return propertyName;
            }
            else
            {
                throw new KeyNotFoundException($"The PropId: {propId} does not correspond with any registered propertyName.");
            }
        }

        /// <summary>
        /// This store accessor holds a reference to a StoreNodeBag; the client IPropBag does not.
        /// Instead of having to lookup the StoreNodeBag that belongs to the client, we can use our reference.
        /// However before doing so we need to make sure that the calling IPropBag does in fact "belong" to this PropStoreAccessService.
        /// 
        /// </summary>
        /// <param name="propBag"></param>
        /// <returns></returns>
        private ObjectIdType GetAndCheckObjectRef(IPropBag propBag)
        {
            if(_clientAccessToken.TryGetTarget(out IPropBag ourPropBag))
            {
                if (!object.ReferenceEquals(propBag, ourPropBag))
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

        private void CheckCompKey(ExKeyT cKey)
        {
            if(cKey.Level1Key != _objectId)
            {
                throw new InvalidOperationException("This PropStoreAccessService can only service the PropBag object that created it.");
            }
        }

        #endregion

        #region Explicit Implementation of the internal interface: IHaveTheStoreNode

        StoreNodeBag IHaveTheStoreNode.PropBagNode => _ourNode;

        #endregion

        #region Explicit Implementation of the internal interface: IPropStoreAccessServiceInternal

        //L2KeyManType PSAccessServiceInternalInterface.Level2KeyManager => _level2KeyMan;

        bool PSAccessServiceInternalInterface.TryGetChildPropNode(StoreNodeBag propBagNode, PropNameType propertyName, out StoreNodeProp child)
        {
            bool result;

            //// TODO: IPBI -- Get Store Accessor from StoreNodeBag
            //if (propBagNode.TryGetPropBag(out IPropBag propBag))
            //{
            //    IPropBagInternal propBagInternal = (IPropBagInternal)propBag;
            //    if (((PSAccessServiceInternalInterface)propBagInternal.ItsStoreAccessor).Level2KeyManager.TryGetFromRaw(propertyName, out PropIdType propId))
            //    {
            //        if (propBagNode.TryGetChild(propId, out child))
            //        {
            //            result = true;
            //        }
            //        else
            //        {
            //            System.Diagnostics.Debug.WriteLine($"Failed to access the child node with property name: {propertyName} on propBagNode: {propBagNode.CompKey}.");
            //            child = null;
            //            result = false;
            //        }
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine($"Could not find that property by name: {propertyName} on propBagNode: {propBagNode.CompKey}.");
            //        child = null;
            //        result = false;
            //    }
            //}
            //else
            //{
            //    System.Diagnostics.Debug.WriteLine($"The weak reference held by this StoreBagNode: {propBagNode.CompKey} holds a reference to an object that is 'no longer with us.'");
            //    child = null;
            //    result = false;
            //}

            if(propBagNode.Level2KeyMan.TryGetFromRaw(propertyName, out PropIdType propId))
            {
                if (propBagNode.TryGetChild(propId, out child))
                {
                    result = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to access the child node with property name: {propertyName} on propBagNode: {propBagNode.CompKey}.");
                    child = null;
                    result = false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Could not find that property by name: {propertyName} on propBagNode: {propBagNode.CompKey}.");
                child = null;
                result = false;
            }

            return result;
        }

        // TODO: Provide method that returns the value of StoreNodeProp.PropData_Internal.TypedProp as IProp<T>
        StoreNodeProp PSAccessServiceInternalInterface.GetChild(PropIdType propId)
        {
            StoreNodeProp result = GetChild(propId);
            return result;
        }

        IDisposable PSAccessServiceInternalInterface.RegisterHandler<T>(PropIdType propId, EventHandler<PcTypedEventArgs<T>> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            SimpleExKey exKey = new SimpleExKey(_objectId, propId);
            IDisposable disable = RegisterHandler<T>(exKey, eventHandler, priorityGroup, keepRef);
            return disable;
        }

        bool PSAccessServiceInternalInterface.UnregisterHandler<T>(PropIdType propId, EventHandler<PcTypedEventArgs<T>> eventHandler)
        {
            SimpleExKey exKey = new SimpleExKey(_objectId, propId);
            return this.UnregisterHandler<T>(exKey, eventHandler);
        }

        IDisposable PSAccessServiceInternalInterface.RegisterHandler(PropIdType propId, EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            SimpleExKey exKey = new SimpleExKey(_objectId, propId);
            IDisposable disable = RegisterHandler(exKey, eventHandler, priorityGroup, keepRef);
            return disable;
        }

        bool PSAccessServiceInternalInterface.UnregisterHandler(PropIdType propId, EventHandler<PcGenEventArgs> eventHandler)
        {
            SimpleExKey exKey = new SimpleExKey(_objectId, propId);
            return this.UnregisterHandler(exKey, eventHandler);
        }

        public bool TryGetPropBagProxy(StoreNodeProp storeNodeProp, out WeakRefKey<IPropBag> propBag_wrKey)
        {
            WeakRefKey<IPropBag>? result = storeNodeProp?.Parent?.PropBagProxy;
            if(result.HasValue)
            {
                propBag_wrKey = result.Value;
                return true;
            }
            else
            {
                propBag_wrKey = new WeakRefKey<IPropBag>(null);
                return false;
            }
        }

        public WeakRefKey<IPropBag> GetPropBagProxy(StoreNodeProp storeNodeProp)
        {
            WeakRefKey<IPropBag>? result = storeNodeProp?.Parent?.PropBagProxy;
            if (result.HasValue)
            {
                return result.Value;
            }
            else
            {
                return new WeakRefKey<IPropBag>(null);
            }
        }

        //WeakReference<IPropBag> PSAccessServiceInternalInterface.GetPublicInterface(WeakReference<IPropBagInternal> x)
        //{
        //    WeakReference<IPropBag> result;

        //    if (x.TryGetTarget(out IPropBagInternal propBagInternal))
        //    {
        //        System.Diagnostics.Debug.Assert(propBagInternal == null || propBagInternal is IPropBag, "This instance of IPropBagInternal does not also implement: IPropBag.");
        //        result = new WeakReference<IPropBag>(propBagInternal as IPropBag);
        //    }
        //    else
        //    {
        //        result = new WeakReference<IPropBag>(null);
        //    }
        //    return result;
        //}

        #endregion

        #region IDisposable Support

        private void ResetAllData()
        {
            if (_propIndexes != null)
            {
                // Remove all handlers from each of our Events.
                int numSubsRemoved = 0;
                foreach (SubscriberCollection sc in _propIndexes)
                {
                    numSubsRemoved += sc.ClearSubscriptions();
                }

                // Clear the contents of our list of Handlers, one for each property.
                _propIndexes.ClearTheListOfSubscriptionPtrs();
            }

            if (_bindings != null)
            {
                // Dispose of each LocalBinding object.
                foreach (ISubscription binding in _bindings)
                {
                    if (binding is IDisposable disable) disable.Dispose();
                }

                // Remove our reference to each LocalBinding.
                int numBindingsRemoved = _bindings.ClearBindings();
            }

            // Note: The _genViewManagers collection doesn't need to be disposed.

            if (_genViewManagerProviders != null)
                _genViewManagerProviders.Dispose();

            // Dispose each PropItem.
            foreach (StoreNodeProp prop in _ourNode.Children)
            {
                prop.PropData_Internal.CleanUp(doTypedCleanup: true);
            }

            _propStoreAccessServiceProvider.TearDown(_ourNode);
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

        public override string ToString()
        {
            return _ourNode.ToString();
        }

        public int AccessCounter => _propStoreAccessServiceProvider.AccessCounter;

        #endregion
    }
}
