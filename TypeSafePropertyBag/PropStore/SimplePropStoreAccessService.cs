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
    using IHaveTheKeyIT = IHaveTheKey<UInt64, UInt64, UInt32>;

    using ICKeyManType = ICKeyMan<UInt64, UInt64, UInt32, String>;
    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    #endregion

    internal class SimplePropStoreAccessService : PSAccessServiceType, IHaveTheKeyIT
    {
        #region Private Members

        readonly WeakReference<IPropBag> _clientAccessToken;
        readonly ObjectIdType _objectId;

        readonly PropStoreNode _ourNode;
        readonly ICKeyManType _compKeyManager;

        PSAccessServiceProviderType _propStoreAccessServiceProvider;

        readonly BindingsCollection _bindings;

        #endregion

        #region Constructor

        public SimplePropStoreAccessService
            (
            PropStoreNode ourNode,
            ICKeyManType compKeyManager,
            PSAccessServiceProviderType propStoreAccessServiceProvider
            )
        {
            _ourNode = ourNode;
            _compKeyManager = compKeyManager;
            _propStoreAccessServiceProvider = propStoreAccessServiceProvider;

            Level2KeyManager = ourNode.PropBagProxy.Level2KeyManager;
            MaxObjectsPerAppDomain = propStoreAccessServiceProvider.MaxObjectsPerAppDomain;

            _clientAccessToken = _ourNode.PropBagProxy.PropBagRef;
            _objectId = _ourNode.PropBagProxy.ObjectId;

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
                CompositeKeyType cKey = GetCompKey(propBag, propId);
                //IPropData result = _theGlobalStore[cKey];
                IPropData result = GetChild(cKey).Int_PropData;
                return result;
            }
            private set
            {
                IPropDataInternal int_PropData = GetInt_PropData(value);
                CompositeKeyType cKey = GetCompKey(propBag, propId);

                //_theGlobalStore[cKey] = value;
                _ourNode.ChildList[cKey].Int_PropData = int_PropData;
            }
        }

        public bool TryGetValue(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            CompositeKeyType cKey = GetCompKey(propBag, propId);
            if (_ourNode.ChildList.TryGetValue(cKey, out PropStoreNode child))
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
            CompositeKeyType propertyKey = GetCompKey(propBag, propId);
            propData = new PropGen(genericTypedProp, propertyKey, propId);
            IPropDataInternal int_PropData = (IPropDataInternal)propData;

            PropStoreNode propStoreNode = new PropStoreNode(propertyKey, int_PropData);
            _ourNode.AddChild(propStoreNode);
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
                    PropStoreNode guestPropBagNode = GetGuestObjectNodeFromNewValue((IPropBag)guest);
                    propStoreNode.AddChild(guestPropBagNode);
                }
            }
            return result;
        }

        public bool TryRemove(IPropBag propBag, PropIdType propId/*, out IPropData propData*/)
        {
            CompositeKeyType cKey = GetCompKey(propBag, propId);
            bool result = _ourNode.ChildList.Remove(cKey);
            return result;
        }
         
        public bool ContainsKey(IPropBag propBag, PropIdType propId)
        {
            CompositeKeyType cKey = GetCompKey(propBag, propId);
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
                _bindings.RemoveBinding(binding.TargetPropId);
            }

            //Bindings = new BindingsCollection();

            foreach (KeyValuePair<CompositeKeyType, PropStoreNode> kvp in _ourNode.Children)
            {
                kvp.Value.Int_PropData.CleanUp(doTypedCleanup: true);
            }
            _ourNode.ChildList.Clear();
        }

        public IEnumerable<KeyValuePair<PropNameType, IPropData>> GetCollection(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);
            IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> propDataObjectsForThisPropBag = _ourNode.Children;
            Dictionary<PropNameType, IPropData> result = propDataObjectsForThisPropBag.ToDictionary
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

        //public void PropHasChanged<T>(IPropBag propBag, PropIdType propId, T oldValue, T newValue, bool oldIsUndefined)
        //{
        //    if (!typeof(T).IsPropBagBased())
        //        return;

        //    PropStoreNode propStoreNode = CheckAndGetChild(propBag, propId, out CompositeKeyType cKey);

        //    if (!oldIsUndefined)
        //    {
        //        // Find the existing objectNode, make it stop being this prop's child, and add it to the top-level list of roots.
        //        PropStoreNode objectChild = GetGuestObjectNodeFromStore(propStoreNode);
        //        objectChild.MakeItARoot();
        //    }

        //    PropStoreNode guestPropBagNode = GetGuestObjectNodeFromNewValue(propBag);
        //    propStoreNode.AddChild(guestPropBagNode);
        //}

        public bool SetTypedProp(IPropBag propBag, PropIdType propId, IProp genericTypedProp)
        {
            CompositeKeyType cKey = GetCompKey(propBag, propId);

            //if (_theGlobalStore.TryGetValue(cKey, out IPropData propData))
            //{
            //    ((IPropDataInternal)propData).SetTypedProp(genericTypedProp);
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

            if(TryGetValue(propBag, propId, out IPropData propData))
            {
                ((IPropDataInternal)propData).SetTypedProp(genericTypedProp);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        private void PropData_PropertyChangedWithObjectVals(object sender, PCObjectEventArgs e)
        {
            if (e.PropertyName == "TypedProp")
            {
                //if(sender is IPropData pd)
                //{
                //    if(pd?.TypedProp?.TypedValueAsObject is IPropBag child)
                //    {
                //        if(pd.ChildObjectId != 0)
                //        {
                //            // Get a reference to the Child's StoreAccessor's IHaveTheSimpleKey interface.
                //            PSAccessServiceType StoreAccessForChild = child.OurStoreAccessor;
                //            // Set the ParentKey on that StoreAccess to the GlobalKey for the PropData item that is being added.

                //            if (StoreAccessForChild is IHaveTheSimpleKey keyAccess)
                //            {
                //                // Get the Global PropId Key using our client's ObjectId, 
                //                ObjectIdType objectId = _compKeyManager.SplitComp(pd.CompKey, out PropIdType propId);
                //                System.Diagnostics.Debug.Assert(objectId == _objectId, "The object ids do not match.");

                //                keyAccess.ParentKey = new SimpleExKey(pd.CompKey, _clientAccessToken, objectId, propId);
                //            }
                //        }

                //    }
                //}
            }
        }

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
            ExKeyT exKey = GetExKey(host, propId);
            SubscriberCollection result = _propStoreAccessServiceProvider.GetSubscriptions(exKey);
            return result;
        }

        public SubscriberCollection GetSubscriptions(ExKeyT exKey)
        {
            SubscriberCollection result = _propStoreAccessServiceProvider.GetSubscriptions(exKey);
            return result;
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

        public bool UnRegisterBinding<T>(IPropBag targetPropBag, PropIdType propId)
        {
            bool result = RemoveBinding(targetPropBag, propId);
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

        public bool RemoveBinding(IPropBag host, uint propId)
        {
            ExKeyT exKey = GetExKey(host, propId);

            bool wasRemoved = _bindings.RemoveBinding(exKey);
            return wasRemoved;
        }

        public ISubscriptionGen GetBinding(IPropBag host, uint propId)
        {
            ExKeyT exKey = GetExKey(host, propId);

            if(_bindings.TryGetBindings(exKey, out ISubscriptionGen binding))
            {
                ISubscriptionGen result = binding;
                return result;                
            }
            else
            {
                throw new InvalidOperationException($"No Binding exists for {exKey.CKey}.");
            }
        }

        #endregion

        #region Private Methods

        private PropStoreNode GetGuestObjectNodeFromStore(PropStoreNode propStoreNode)
        {
            System.Diagnostics.Debug.Assert(propStoreNode.IsObjectNode, "Attempting to call GetObjectChild on a node that is not an ObjectNode.");

            PropStoreNode childObjectNode = propStoreNode.ChildList[0];
            return childObjectNode;
        }

        private PropStoreNode GetGuestObjectNodeFromNewValue(IPropBag propBag)
        {
            //ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            if (propBag is IPropBagInternal pbInternalAccess)
            {
                PSAccessServiceType accessService = pbInternalAccess.OurStoreAccessor;
                if (accessService is IHaveTheKeyIT itsGotTheKey)
                {
                    PropStoreNode propStoreNode = itsGotTheKey.PropStoreNode;
                    return propStoreNode;
                }
                else
                {
                    throw new InvalidOperationException($"The {nameof(propBag)}'s {nameof(pbInternalAccess.OurStoreAccessor)} does not implement the {nameof(IHaveTheKeyIT)} interface.");
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

        //private IPropData GetChild(CompositeKeyType cKey)
        //{
        //    if (_ourNode.TryGetChild(cKey, out PropStoreNode child))
        //    {
        //        IPropData result = _ourNode.Int_PropData;
        //        return result;
        //    }
        //    else
        //    {
        //        throw new KeyNotFoundException("That propId could not be found.");
        //    }
        //}

        private PropStoreNode GetChild(CompositeKeyType cKey)
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

        private PropStoreNode CheckAndGetChild(IPropBag propBag, PropIdType propId, out CompositeKeyType cKey)
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

        CompositeKeyType GetCompKey(IPropBag propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            CompositeKeyType cKey = _compKeyManager.JoinComp(objectId, propId);

            return cKey;
        }

        private string GetPropNameFromKey(CompositeKeyType cKey)
        {
            ObjectIdType objectId = _compKeyManager.SplitComp(cKey, out PropIdType propId);

            if(Level2KeyManager.TryGetFromCooked(propId, out PropNameType propertyName))
            {
                return propertyName;
            }
            else
            {
                throw new KeyNotFoundException($"The cKey: {cKey}, which included PropId: {propId} does not correspond with any registered propertyName.");
            }
        }

        private ObjectIdType GetObjectIdFromKey(CompositeKeyType cKey)
        {
            ObjectIdType objectId = _compKeyManager.SplitComp(cKey, out PropIdType propId);
            return objectId;
        }

        private ExKeyT GetExKey(IPropBag propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            CompositeKeyType cKey = _compKeyManager.JoinComp(objectId, propId);
            SimpleExKey exKey = new SimpleExKey(cKey, _clientAccessToken, objectId, propId);
            return exKey;
        }

        private SimpleExKey GetExKey(IPropBag propBag, CompositeKeyType cKey)
        {
            ObjectIdType objectIdFromCKey = _compKeyManager.SplitComp(cKey, out PropIdType propId);
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            if(!object.ReferenceEquals(objectId, objectIdFromCKey))
            {
                throw new InvalidOperationException("The composite key given does not match the value given for propBag.");

            }

            SimpleExKey exKey = new SimpleExKey(cKey, _clientAccessToken, objectId, propId);
            return exKey;
        }

        /// <summary>
        /// Does not check the ObjectReference.
        /// </summary>
        /// <param name="cKey"></param>
        /// <param name="propId"></param>
        /// <returns></returns>
        private SimpleExKey GetExKey(CompositeKeyType cKey, PropIdType propId)
        {
            SimpleExKey exKey = new SimpleExKey(cKey, _clientAccessToken, _objectId, propId);
            return exKey;
        }

        // Also verifies that the compKey matches the propBag value.
        // Don't use this if you need an ExKey, since 1) ExKey requires a PropId value, 2) to get a PropId value you need split the CompKey, 3) Verify Splits the CompKey, 4) you can easily verify by using reference equality to see that our _objectId == the objectId from the CompKey.
        private ObjectIdType GetAndCheckObjectRef(IPropBag propBag, CompositeKeyType cKey)
        {
            ObjectIdType result = GetAndCheckObjectRef(propBag);
            if(!_compKeyManager.Verify(cKey, result))
            {
                throw new InvalidOperationException("The composite key given does not match the value given for propBag.");
            }

            return result;
        }

        private ObjectIdType GetAndCheckObjectRef(IPropBag propBag)
        {
            IPropBag client = SimpleExKey.UnwrapWeakRef(_clientAccessToken);
            if (client == null)
            {
                throw new InvalidOperationException("The weak reference to the PropBag held by the StoreAccessor, holds no object.");
            }

            if (!object.ReferenceEquals(propBag, client))
            {
                throw new InvalidOperationException("This PropStoreAccessService can only service the PropBag object that created it.");
            }

            ObjectIdType result = _objectId;

            return result;
        }

        #endregion

        #region Explicit Implementation of the internal interface: IHaveTheKey

        public ObjectIdType ObjectId => _objectId;

        ExKeyT IHaveTheKeyIT.GetTheKey(IPropBag propBag, PropIdType propId)
        {
            ExKeyT result = GetExKey(propBag, propId);
            return result;
        }

        ExKeyT IHaveTheKeyIT.GetTheKey(IPropBagProxy propBagProxy, PropIdType propId)
        {
            CompositeKeyType cKey = _compKeyManager.JoinComp(_objectId, propId);
            ExKeyT result = new SimpleExKey(cKey, propBagProxy.PropBagRef, _objectId, propId);
            return result;
        }

        PropStoreNode IHaveTheKeyIT.PropStoreNode => _ourNode;

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
