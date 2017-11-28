using System;
using System.Collections.Generic;
using System.Linq;

using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases 
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    #endregion

    internal class SimplePropStoreAccessService : PSAccessServiceType, IHaveTheStoreNode
    {
        #region Private Members

        readonly WeakReference<IPropBagInternal> _clientAccessToken;
        readonly ObjectIdType _objectId;

        readonly PropStoreNode _ourNode;

        PSAccessServiceProviderType _propStoreAccessServiceProvider;

        readonly BindingsCollection _bindings;

        #endregion

        #region Constructor

        public SimplePropStoreAccessService
            (
            PropStoreNode ourNode,
            L2KeyManType level2KeyManager,
            PSAccessServiceProviderType propStoreAccessServiceProvider
            )
        {
            _ourNode = ourNode;
            _propStoreAccessServiceProvider = propStoreAccessServiceProvider;

            Level2KeyManager = level2KeyManager; //ourNode.PropBagProxy.Level2KeyManager;
            MaxObjectsPerAppDomain = propStoreAccessServiceProvider.MaxObjectsPerAppDomain;

            _clientAccessToken = _ourNode.PropBagProxy.PropBagRef;
            _objectId = _ourNode.CompKey.Level1Key; // .PropBagProxy.ObjectId;

            _bindings = new BindingsCollection();
        }

        #endregion

        #region Public Members

        public L2KeyManType Level2KeyManager { get; }

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
            private set
            {
                IPropDataInternal int_PropData = GetInt_PropData(value);
                ExKeyT cKey = GetCompKey(propBag, propId);

                _ourNode.ChildList[cKey].Int_PropData = int_PropData;
            }
        }

        public bool TryGetValue(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            ExKeyT cKey = GetCompKey(propBag, propId);
            if(_ourNode.TryGetChild(cKey, out PropStoreNode child))
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

        public bool TryAdd(IPropBag propBag, PropIdType propId, PropNameType propertyName, IProp genericTypedProp, out IPropData propData)
        {
            ExKeyT propertyKey = GetCompKey(propBag, propId);
            propData = new PropGen(genericTypedProp, propertyKey, propId, propertyName);
            IPropDataInternal int_PropData = (IPropDataInternal)propData;

            PropStoreNode propStoreNode = new PropStoreNode(propertyKey, int_PropData, _ourNode);
            bool result = true;

            if (int_PropData.IsPropBag)
            {
                // If the new property is of a type that implements IPropBag,
                // attempt to get a reference to the StoreAccessor of that IPropBag object,
                // and from that StoreAccessor, the object id of the PropBag.
                // Use the PropBag's ObjectId to set the ChildObjectId of this new property.

                object guest = int_PropData?.TypedProp?.TypedValueAsObject; 
                if(guest != null)
                {
                    PropStoreNode guestPropBagNode = GetGuestObjectNodeFromPropItemVal((IPropBag)guest);
                    guestPropBagNode.MakeItAChildOf(propStoreNode);
                }

                // Subscribe to changes to this PropData's Value.
                BeginWatchingParent(propertyKey, propId);
            }
            return result;
        }

        public bool TryRemove(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            ExKeyT cKey = GetCompKey(propBag, propId);

            bool result = _ourNode.TryRemoveChild(cKey, out PropStoreNode child);

            if (result)
                propData = child.Int_PropData;
            else
                propData = null;

            return result;
        }
         
        public bool ContainsKey(IPropBag propBag, PropIdType propId)
        {
            ExKeyT cKey = GetCompKey(propBag, propId);
            bool result = _ourNode.ChildList.ContainsKey(cKey);
            return result;
        }

        // TODO: Take a lock here.
        public void Clear(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);
            ClearInt();
        }

        public void Destroy()
        {
            ClearInt();
            //readonly WeakReference<IPropBag> _clientAccessToken;
            //readonly ObjectIdType _objectId;

            //readonly PropStoreNode _ourNode;
            //readonly ICKeyManType _compKeyManager;
            //readonly L2KeyManType _level2KeyManager;

            //readonly PSAccessServiceProviderType _propStoreAccessServiceProvider;

            //readonly BindingsCollection _bindings;
            _propStoreAccessServiceProvider = null;
        }

        private void ClearInt()
        {
            foreach (ISubscriptionGen binding in _bindings)
            {
                // TODO: Implement IDisposable on ISubscriptionGen
                //binding.Clear();
                _bindings.TryRemoveBinding(binding);
            }

            //Bindings = new BindingsCollection();

            foreach (KeyValuePair<ExKeyT, PropStoreNode> kvp in _ourNode.Children)
            {
                kvp.Value.Int_PropData.CleanUp(doTypedCleanup: true);
            }
            _ourNode.ChildList.Clear();
        }

        public IEnumerable<KeyValuePair<PropNameType, IPropData>> GetCollection(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);
            Dictionary<PropNameType, IPropData> result = _ourNode.Children.ToDictionary
                (
                x => GetPropNameFromKey(x.Key),
                x => (IPropData) x.Value.Int_PropData
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
            IEnumerable<PropNameType> result = _ourNode.Children.Select(x => GetPropNameFromKey(x.Key));
            return result;
        }

        public IEnumerable<IPropData> GetValues(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);
            IEnumerable<IPropData> result = _ourNode.Children.Select(x => x.Value.Int_PropData);
            return result;
        }

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
                        StopWatchingParent(cKey, propId);
                    }

                    int_propData.SetTypedProp(propertyName, genericTypedProp);

                    // TODO: Raise the Prop Changed event.
                    //OnPropertyChangedWithObjectVals(propertyName, oldTypedProp?.TypedValueAsObject, genericTypedProp?.TypedValueAsObject);

                    if (genericTypedProp.TypedValueAsObject != null && genericTypedProp.TypedValueAsObject.GetType().IsPropBagBased())
                    {
                        // Subscribe to the new value's PropertyChanged event.
                        BeginWatchingParent(cKey, propId);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IRegisterSubscriptions Implementation

        public bool RegisterHandler<T>(IPropBag propBag, PropIdType propId, 
            EventHandler<PCTypedEventArgs<T>> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetExKey(propBag, propId);

            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKey<T>(exKey, eventHandler, priorityGroup, keepRef);

            ISubscriptionGen newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);
            return wasAdded;
        }

        public bool UnRegisterHandler<T>(IPropBag propBag, PropIdType propId, EventHandler<PCTypedEventArgs<T>> eventHandler)
        {
            ExKeyT exKey = GetExKey(propBag, propId);

            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKey<T>(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        public bool RegisterHandler(IPropBag propBag, uint propId, 
            EventHandler<PCGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, priorityGroup, keepRef);

            ISubscriptionGen newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);
            return wasAdded;
        }

        public bool UnRegisterHandler(IPropBag propBag, uint propId, EventHandler<PCGenEventArgs> eventHandler)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        public bool RegisterHandler(IPropBag propBag, uint propId, 
            EventHandler<PCObjectEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, priorityGroup, keepRef);

            ISubscriptionGen newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);
            return wasAdded;
        }

        public bool UnRegisterHandler(IPropBag propBag, uint propId, EventHandler<PCObjectEventArgs> eventHandler)
        {
            ExKeyT exKey = GetExKey(propBag, propId);
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);

            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        #endregion

        #region ICacheSubscriptions Implementation -- Pass through to the Service Provider.

        public ISubscriptionGen AddSubscription(ISubscriptionKeyGen subscriptionRequest, out bool wasAdded)
        {
            ISubscriptionGen result = _propStoreAccessServiceProvider.AddSubscription(subscriptionRequest, out wasAdded);
            return result;
        }

        public bool RemoveSubscription(ISubscriptionKeyGen subscriptionRequest)
        {
            bool result = _propStoreAccessServiceProvider.RemoveSubscription(subscriptionRequest);
            return result;
        }

        public SubscriberCollection GetSubscriptions(IPropBag host, PropIdType propId)
        {
            //ExKeyT exKey = GetExKey(host, propId);

            // TODO: NOTE: This does not verify that the caller is the "correct" one.
            ExKeyT exKey = new SimpleExKey(_objectId, propId);

            if( _propStoreAccessServiceProvider.TryGetSubscriptions(exKey, out SubscriberCollection subs))
            {
                return subs;
            }
            else
            {
                return null;
            }
            
            //SubscriberCollection result = _propStoreAccessServiceProvider.GetSubscriptions(_objectId, propId);
            //return result;
        }

        //public SubscriberCollection GetSubscriptions(ObjectIdType objectId, PropIdType propId)
        //{
        //    throw new NotSupportedException("GetSubscriptions with ObjectId and PropId is not supported. Use the version that takes a IPropBag and propId.");
        //    //SubscriberCollection result = _propStoreAccessServiceProvider.GetSubscriptions(objectId, propId);
        //    //return result;
        //}

        public bool TryGetSubscriptions(ExKeyT exKey, out SubscriberCollection subscriberCollection)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IRegisterBindings Implementation

        public bool RegisterBinding<T>(IPropBag targetPropBag, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            ExKeyT exKey = GetExKey(targetPropBag, propId);
            ISubscriptionKeyGen BindingRequest = new BindingSubscriptionKey<T>(exKey, bindingInfo);
            ISubscriptionGen newSubscription = AddBinding(BindingRequest, out bool wasAdded);
            return wasAdded;
        }

        public bool UnRegisterBinding<T>(IPropBag targetPropBag, PropIdType propId, LocalBindingInfo bindingInfo)
        {
            ExKeyT exKey = GetExKey(targetPropBag, propId);
            ISubscriptionKeyGen bindingRequest = new BindingSubscriptionKey<T>(exKey, bindingInfo);
            
            bool result = TryRemoveBinding(bindingRequest, out ISubscriptionGen binding);
            return result;
        }

        #endregion

        #region Binding Management

        public ISubscriptionGen AddBinding(ISubscriptionKeyGen bindingRequest, out bool wasAdded)
        {
            if (bindingRequest.HasBeenUsed)
            {
                throw new ApplicationException("Its already been used.");
            }

            ISubscriptionGen result = _bindings.GetOrAdd(bindingRequest, x => bindingRequest.CreateBinding(this));

            if (bindingRequest.HasBeenUsed)
            {
                System.Diagnostics.Debug.WriteLine($"Created a new Binding for Property:" +
                    $" {bindingRequest.TargetPropRef} / Event: {result.SubscriptionKind}.");
                wasAdded = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"The Binding for Property:" +
                    $" {bindingRequest.TargetPropRef} / Event: {result.SubscriptionKind} was not added.");
                wasAdded = false;
            }

            return result;

        }

        public bool TryRemoveBinding(ISubscriptionKeyGen bindingRequest, out ISubscriptionGen binding)
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

        public IEnumerable<ISubscriptionGen> GetBindings(IPropBag host, uint propId)
        {
            ExKeyT exKey = GetExKey(host, propId);

            IEnumerable<ISubscriptionGen> result = _bindings.TryGetBindings(exKey);
            return result;                
        }

        #endregion

        #region Prop Store Node Events and Call Backs

        private bool BeginWatchingParent(ExKeyT propertyKey, PropIdType propId)
        {
            SubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyKey, PropertyChangedWithObjectVals, SubscriptionPriorityGroup.Standard, keepRef: false);
            AddSubscription(subscriptionRequest, out bool wasAdded);
            return wasAdded;
        }

        private bool StopWatchingParent(ExKeyT propertyKey, PropIdType propId)
        {

            SubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyKey, PropertyChangedWithObjectVals, SubscriptionPriorityGroup.Standard, keepRef: false);
            bool wasRemoved = RemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        // Our call back to let us know to update the parentage of a IPropBag object.
        private void PropertyChangedWithObjectVals(object sender, PCObjectEventArgs e)
        {
            if (e.NewValueIsUndefined)
            {
                if (e.OldValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.OldValue.GetType().IsPropBagBased(), "The old value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Make old a root.
                    PropStoreNode guestPropBagNode = GetGuestObjectNodeFromPropItemVal((IPropBag)e.OldValue);
                    _ourNode.AddSibling(guestPropBagNode);
                }
            }
            else if (e.OldValueIsUndefined)
            {
                if (e.NewValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.NewValue.GetType().IsPropBagBased(), "The new value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Move to child of our property item. This object is currently a root.
                    IPropBag propBagHost = (IPropBag)e.NewValue;

                    PropStoreNode guestPropBagNode = GetGuestObjectNodeFromPropItemVal(propBagHost);

                    //if (_ourNode.PropBagProxy.Level2KeyManager.TryGetFromRaw(e.PropertyName, out PropIdType propId))

                    if (((IPropBagInternal)propBagHost).Level2KeyManager.TryGetFromRaw(e.PropertyName, out PropIdType propId))
                    {
                        ExKeyT cKey = new SimpleExKey(_objectId, propId);
                        PropStoreNode propItemNode = GetChild(cKey);
                        guestPropBagNode.MakeItAChildOf(propItemNode);
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
                    PropStoreNode guestPropBagNode = GetGuestObjectNodeFromPropItemVal((IPropBag)e.OldValue);
                    _ourNode.AddSibling(guestPropBagNode);
                }

                if (e.NewValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.NewValue.GetType().IsPropBagBased(), "The new value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Move to child of our property item. This object is currently a root.
                    IPropBag propBagHost = (IPropBag)e.NewValue;

                    PropStoreNode guestPropBagNode = GetGuestObjectNodeFromPropItemVal(propBagHost);

                    //if (_ourNode.PropBagProxy.Level2KeyManager.TryGetFromRaw(e.PropertyName, out PropIdType propId))

                    if (((IPropBagInternal)propBagHost).Level2KeyManager.TryGetFromRaw(e.PropertyName, out PropIdType propId))
                    {
                        PropStoreNode propItemNode = GetChild(propId);
                        if(guestPropBagNode.Parent != propItemNode)
                        {
                            guestPropBagNode.MakeItAChildOf(propItemNode);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("The PropItemNode is already our parent.");
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private PropStoreNode GetGuestObjectNodeFromStore(PropStoreNode propStoreNode)
        {
            //System.Diagnostics.Debug.Assert(!propStoreNode.IsObjectNode, "Attempting to call GetGuestObjectNodeFromStore on a node that is an ObjectNode.");

            PropStoreNode childObjectNode = propStoreNode.OnlyChildOfPropItem;
            return childObjectNode;
        }

        private PropStoreNode GetGuestObjectNodeFromPropItemVal(IPropBag propBag)
        {
            //ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            if (propBag is IPropBagInternal pbInternalAccess)
            {
                PSAccessServiceType accessService = pbInternalAccess.ItsStoreAccessor;
                if (accessService is IHaveTheStoreNode itsGotTheKey)
                {
                    PropStoreNode propStoreNode = itsGotTheKey.PropStoreNode;
                    System.Diagnostics.Debug.Assert(propStoreNode.IsObjectNode, "The propStoreNode returned from GetGuestObjectNodeFromPropVal should have IsObjectNode = true.");
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

        PropStoreNode _root;
        private PropStoreNode Root
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

        private PropStoreNode GetChild(ExKeyT cKey)
        {
            if (_ourNode.TryGetChild(cKey, out PropStoreNode child))
            {
                PropStoreNode result = child;
                return result;
            }
            else
            {
                throw new KeyNotFoundException("That propId could not be found.");
            }
        }

        private PropStoreNode GetChild(PropIdType propId)
        {
            ExKeyT cKey = new SimpleExKey(this._objectId, propId);
            if (_ourNode.TryGetChild(cKey, out PropStoreNode child))
            {
                PropStoreNode result = child;
                return result;
            }
            else
            {
                throw new KeyNotFoundException("That propId could not be found.");
            }
        }

        private PropStoreNode CheckAndGetChild(IPropBag propBag, PropIdType propId, out ExKeyT cKey)
        {
            cKey = GetCompKey(propBag, propId);
            PropStoreNode result = GetChild(cKey);
            return result;
        }

        private IPropBagProxy GetInt_PropBag(IPropBag propBagWithInt)
        {
            if (propBagWithInt is IPropBagProxy ipbi)
            {
                return ipbi;
            }
            else
            {
                throw new ArgumentException($"int_PropBag does not implement {nameof(IPropBagProxy)}.");
            }
        }

        private IPropDataInternal GetInt_PropData(IPropData propDataWithInt)
        {
            if (propDataWithInt is IPropDataInternal int_PropData)
            {
                return int_PropData;
            }
            else
            {
                throw new ArgumentException($"propDataWithInt does not implement {nameof(propDataWithInt)}.");
            }
        }

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

        PropStoreNode IHaveTheStoreNode.PropStoreNode => _ourNode;

        //ObjectIdType IHaveTheKeyIT.ObjectId => _objectId;

        //ExKeyT IHaveTheKeyIT.GetTheKey(IPropBag propBag, PropIdType propId)
        //{
        //    ExKeyT result = GetExKey(propBag, propId);
        //    return result;
        //}

        //PropStoreNode IHaveTheKeyIT.GetObjectNodeForPropVal(IPropDataInternal int_propData)
        //{
        //    System.Diagnostics.Debug.Assert(int_propData != null, "Any parent of an ObjectId type PropStoreNode should have an instance of an IPropDataInternal.");
        //    System.Diagnostics.Debug.Assert(int_propData.TypedProp != null, "All objects that implement IPropDataInternal must have a non-null value for TypedProp.");
        //    System.Diagnostics.Debug.Assert(int_propData.TypedProp.Type is IPropBag, "All calls to GetObjectNodeForPropVal must be made for properties of a type that derives from IPropBag.");

        //    object test = int_propData.TypedProp.TypedValueAsObject;

        //    if (test == null) return null;

        //    System.Diagnostics.Debug.Assert(test is IPropBagInternal, "All instances of IPropBag must also implement IPropBagInternal.");
        //    System.Diagnostics.Debug.Assert(((IPropBagInternal)test).ItsStoreAccessor != null, "All instances of IPropBagInternal must have a non-null value for ItsStoreAccessor.");
        //    System.Diagnostics.Debug.Assert(((IPropBagInternal)test).ItsStoreAccessor is IHaveTheKeyIT, "All instances of IPropBagInternal must have a value for ItsStoreAccessor that implements IHaveTheKeyIT.");

        //    PropStoreNode result = ((IHaveTheKeyIT)((IPropBagInternal)test).ItsStoreAccessor).PropStoreNode;
        //    return result;
        //}

        //bool IHaveTheKeyIT.TryGetAChildOfMine(PropIdType propId, out PropStoreNode child)
        //{
        //    bool result = _ourNode.TryGetChild(propId, out child);
        //    return result;
        //}

        #endregion

        #region Diagnostics

        public void IncAccess()
        {
            _propStoreAccessServiceProvider.IncAccess();
        }

        public int AccessCounter => _propStoreAccessServiceProvider.AccessCounter;

        #endregion

    }
}
