using DRM.TypeSafePropertyBag.DataAccessSupport;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeSafePropertyBag.LocalBinding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;
    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;

    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    using PropNodeCollectionIntInterface = IPropNodeCollection_Internal<UInt32, String>;
    using PropNodeCollectionInterface = IPropNodeCollection<UInt32, String>;

    using PropModelType = IPropModel<String>;

    internal class SimplePropStoreAccessService : PSAccessServiceInterface, IHaveTheStoreNode, PSAccessServiceInternalInterface, IDisposable
    {
        #region Unused Operational Constants
        //// Subscription Management
        //const int OBJECT_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        //const int EXPECTED_NO_OF_OBJECTS = 10000;
        #endregion

        #region Private Members
    
        // Use a single instance of a WeakReference to this instance for all 
        readonly WeakReference<PSAccessServiceInterface> _wrThis;

        readonly BagNode _ourNode;

        WeakRefKey<IPropBag> _clientAccessToken { get; }
        ObjectIdType _objectId { get; }

        PSAccessServiceProviderInterface _propStoreAccessServiceProvider;
        readonly IProvideHandlerDispatchDelegateCaches _handlerDispatchDelegateCacheProvider;

        // Each PropItem has a collection of Subscribers for a variety of event types.
        private CollectionOfSubscriberCollections _subscriberCollections;

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
            BagNode ourNode,
            PSAccessServiceProviderInterface propStoreAccessServiceProvider,
            IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider
            )
        {
            _ourNode = ourNode;
            _clientAccessToken = _ourNode.PropBagProxy;
            _objectId = ourNode.ObjectId;

            _propStoreAccessServiceProvider = propStoreAccessServiceProvider;
            _handlerDispatchDelegateCacheProvider = handlerDispatchDelegateCacheProvider;

            _wrThis = new WeakReference<PSAccessServiceInterface>(this);

            _subscriberCollections = null; 
            _bindings = null; 
            _genViewManagers = null;
            _genViewManagerProviders = null;
        }

        #endregion

        #region Public Members

        public int MaxPropsPerObject => _propStoreAccessServiceProvider.MaxPropsPerObject; 
        public long MaxObjectsPerAppDomain => _propStoreAccessServiceProvider.MaxObjectsPerAppDomain;

        public PSAccessServiceCreatorInterface StoreAcessorCreator => _propStoreAccessServiceProvider.StoreAcessorCreator;

        public IPropData this[IPropBag propBag, PropIdType propId]
        {
            get
            {
                CheckObjectRef(propBag);
                //ExKeyT cKey = GetCompKey(propBag, propId);
                IPropData result = GetChild(propId).PropData_Internal;
                return result;
            }
        }

        public bool TryGetValue(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            CheckObjectRef(propBag);
            if (_ourNode.TryGetChild(propId, out PropNode child))
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
        public bool TryAdd(IPropBag propBag, PropNameType propertyName, IProp genericTypedProp,
            out IPropData propData, out PropIdType propId)
        {
            CheckObjectRef(propBag);

            PropNode newNode = AddPropItemToStore(genericTypedProp, propertyName);
            propData = newNode.PropData_Internal;
            propId = newNode.PropId;

            if (newNode.HoldsAPropBag)
            {
                HookupGuestBagNode(newNode);
            }

            if (propData.TypedProp.PropTemplate.StorageStrategy == PropStorageStrategyEnum.Virtual)
            {
                HookupStandardPropertyChangedHandler(newNode, propertyName);
            }

            return true;
        }

        // Add PropItem with PcTyped subscription
        public bool TryAdd<PropT>(IPropBag propBag, PropNameType propertyName, IProp genericTypedProp,
            EventHandler<PcTypedEventArgs<PropT>> eventHandler, SubscriptionPriorityGroup priorityGroup,
            out IPropData propData, out PropIdType propId)
        {
            CheckObjectRef(propBag);

            PropNode newNode = AddPropItemToStore(genericTypedProp, propertyName);
            propData = newNode.PropData_Internal;
            propId = newNode.PropId;

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

            if (newNode.HoldsAPropBag)
            {
                HookupGuestBagNode(newNode);
            }

            if (propData.TypedProp.PropTemplate.StorageStrategy == PropStorageStrategyEnum.Virtual)
            {
                HookupStandardPropertyChangedHandler(newNode, propertyName);
            }

            return result;
        }

        // Add PropItem with Target/Method subscription
        public bool TryAdd(IPropBag propBag, PropNameType propertyName, IProp genericTypedProp,
            object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup,
            out IPropData propData, out PropIdType propId)
        {
            CheckObjectRef(propBag);

            PropNode newNode = AddPropItemToStore(genericTypedProp, propertyName);
            propData = newNode.PropData_Internal;
            propId = newNode.PropId;

            bool result;
            if (target != null)
            {
                ISubscriptionKeyGen subscriptionRequest =
                    new SubscriptionKeyGen(newNode.CompKey, genericTypedProp.PropTemplate.Type, target, method, subscriptionKind, priorityGroup, keepRef: false, subscriptionFactory: null);

                ISubscription newSubscription = AddSubscription(subscriptionRequest, out bool wasAdded);

                result = wasAdded;
            }
            else
            {
                result = true;
            }

            if (newNode.HoldsAPropBag)
            {
                HookupGuestBagNode(newNode);
            }

            if (propData.TypedProp.PropTemplate.StorageStrategy == PropStorageStrategyEnum.Virtual)
            {
                HookupStandardPropertyChangedHandler(newNode, propertyName);
            }

            return result;
        }

        // Add PropItem with PcGen subscription
        public bool TryAdd(IPropBag propBag, PropNameType propertyName, IProp genericTypedProp,
            EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup,
            out IPropData propData, out PropIdType propId)
        {
            CheckObjectRef(propBag);

            PropNode newNode = AddPropItemToStore(genericTypedProp, propertyName);
            propData = newNode.PropData_Internal;
            propId = newNode.PropId;

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

            if (newNode.HoldsAPropBag)
            {
                HookupGuestBagNode(newNode);
            }

            if (propData.TypedProp.PropTemplate.StorageStrategy == PropStorageStrategyEnum.Virtual)
            {
                HookupStandardPropertyChangedHandler(newNode, propertyName);
            }

            return result;
        }

        private PropNode AddPropItemToStore(IProp genericTypedProp, PropNameType propertyName)
        {
            IPropDataInternal propData_Internal = new PropGen(genericTypedProp);

            if (IsPropItemSetFixed)
            {
                if (!TryOpenPropItemSet(/*out object propItemSet_Handle_NotUsed*/))
                {
                    throw new InvalidOperationException("Can not add props to a fixed PropItemSet, and the attempt to open the PropItemSet failed.");
                }
            }

            PropNode propStoreNode = _ourNode.CreateAndAddPropNode(propData_Internal, propertyName);

            return propStoreNode;
        }

        private void HookupGuestBagNode(PropNode propStoreNode)
        {
            // If the new property is of a type that implements IPropBag,
            // attempt to get a reference to the StoreAccessor of that IPropBag object,
            // and from that StoreAccessor, the object id of the PropBag.
            // Use the PropBag's ObjectId to set the ChildObjectId of this new property.

            object guest = propStoreNode.PropData_Internal?.TypedProp?.TypedValueAsObject;
            if (guest != null)
            {
                BagNode guestPropBagNode = GetGuestBagNodeFromPropItemVal(guest);
                guestPropBagNode.Parent = propStoreNode;
            }

            // Subscribe to changes to this PropData's Value.
            BeginWatchingGuest(propStoreNode.CompKey);
        }

        private void HookupStandardPropertyChangedHandler(PropNode propNode, PropNameType propertyName)
        {
            EventHandler<EventArgs> virtualValueChangedHandler = CreateVirtualValueChangedHandler(propNode, propertyName);
            if (virtualValueChangedHandler != null)
            {
                propNode.PropData_Internal.TypedProp.ValueChanged += virtualValueChangedHandler;
            }
        }

        private EventHandler<EventArgs> CreateVirtualValueChangedHandler(PropNode propNode, PropNameType propertyName)
        {
            BagNode hostBagNode = propNode?.Parent;

            if (hostBagNode == null) return null;

            WeakRefKey<IPropBag> propBagRef = hostBagNode.PropBagProxy;
            PropIdType propId = propNode.PropId;

            return newHandler;

            void newHandler(object sender, EventArgs e)
            {
                this.ForwardRequestToRaiseStandardPC(propBagRef, propertyName);
            }
        }

        private void ForwardRequestToRaiseStandardPC(WeakRefKey<IPropBag> propBagRef, PropNameType propName)
        {
            if (propBagRef.TryGetTarget(out IPropBag propBag))
            {
                propBag.RaiseStandardPropertyChanged(propName);
            }
        }

        public bool TryRemove(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            if (IsPropItemSetFixed)
            {
                if (!TryOpenPropItemSet(/*out object propItemSet_Handle_NotUsed*/))
                {
                    throw new InvalidOperationException("Can not remove props to a fixed PropItemSet, and the attempt to open the PropItemSet failed.");
                }
            }

            bool result = _ourNode.TryRemoveChild(propId, out PropNode child);

            if (result)
            {
                child.PropData_Internal.CleanUp(doTypedCleanup: true);
                propData = child.PropData_Internal;
            }
            else
            {
                propData = null;
            }

            return result;
        }

        public bool ContainsKey(IPropBag propBag, PropIdType propId)
        {
            //ExKeyT cKey = GetCompKey(propBag, propId);
            bool result = _ourNode.ChildExists(propId);
            return result;
        }

        public string GetClassAndPropName(ExKeyT compKey)
        {
            string result;

            if (compKey.Level1Key == _objectId)
            {
                // The composite key identifies a PropItem that belongs to our BagNode.

                if (TryGetPropBagForOurNode(out IPropBag ourPropBag))
                {
                    if (compKey.Level2Key == 0)
                    {
                        result = $"{ourPropBag.FullClassName}::All Props";
                    }
                    else
                    {
                        if (TryGetPropName(compKey.Level2Key, out PropNameType propertyName))
                        {
                            result = $"{ourPropBag.FullClassName}::{propertyName}";
                        }
                        else
                        {
                            result = $"Could not find a propertyName for PropId = {compKey}; class name = {ourPropBag.FullClassName}.";
                        }
                    }
                }
                else
                {
                    result = $"Could not retrieve the IPropBag that is being serviced by this StoreAccessor. CompKey = {compKey}";
                }
            }
            else
            {
                // The PropItem belongs to some other Store Acessor's BagNode.
                throw new NotImplementedException($"{nameof(GetClassAndPropName)} does not know how to handle composite keys that belong to a 'foreign' store accessor.");
            }

            return result;
        }

        public string GetFullClassNameForOurNode()
        {
            string result;

            if (TryGetPropBagForOurNode(out IPropBag ourPropBag))
            {
                result = ourPropBag.FullClassName;
            }
            else
            {
                result = $"Could not retrieve the IPropBag that is being serviced by this StoreAccessor. CompKey = {new SimpleExKey(_objectId, 0)}";
            }

            return result;
        }

        public bool TryGetFullClassNameForOurNode(out string fullClassName)
        {
            if (TryGetPropBagForOurNode(out IPropBag ourPropBag))
            {
                fullClassName = ourPropBag.FullClassName;
                return true;
            }
            else
            {
                fullClassName = null;
                return false;
            }
        }

        public bool TryGetFullClassName(BagNode propBagNode, out string fullClassName)
        {
            if(propBagNode.TryGetPropBag(out IPropBag propBag))
            {
                fullClassName = propBag.FullClassName;
                return true;
            }
            else
            {
                fullClassName = null;
                return false;
            }
        }

        public bool TryGetParentPropBagProxy(PropNode storeNodeProp, out WeakRefKey<IPropBag> propBag_wrKey)
        {
            WeakRefKey<IPropBag>? result = storeNodeProp?.Parent?.PropBagProxy;
            if (result.HasValue)
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

        public bool TryGetPropBagForOurNode(out IPropBag ourPropBag)
        {
            bool result = _clientAccessToken.TryGetTarget(out ourPropBag);
            return result;
        }

        public bool TryGetPropBag(PSAccessServiceInterface propStoreAccessService, out IPropBag propBag)
        {
            BagNode propBagNode = GetPropBagNode(propStoreAccessService);

            bool result = propBagNode.TryGetPropBag(out propBag);
            return result;
        }

        public IReadOnlyDictionary<PropNameType, IPropData> GetCollection(IPropBag propBag)
        {
            CheckObjectRef(propBag);

            IReadOnlyDictionary<PropNameType, IPropData> result = _ourNode.GetPropDataItemsDict();

            //Dictionary<PropNameType, IPropData> result = _ourNode.Children.ToDictionary
            //    (
            //    x => GetPropNameFromLocalPropId(x.CompKey.Level2Key),
            //    x => (IPropData)x.PropData_Internal
            //    );

            return result;
        }

        public IEnumerator<KeyValuePair<PropNameType, IPropData>> GetEnumerator(IPropBag propBag)
        {
            CheckObjectRef(propBag);

            IEnumerator<KeyValuePair<PropNameType, IPropData>> result = GetCollection(propBag).GetEnumerator();

            return result;
        }

        public IEnumerable<PropNameType> GetKeys(IPropBag propBag)
        {
            CheckObjectRef(propBag);
            IEnumerable<PropNameType> result = _ourNode.GetPropNames(); // .Children.Select(x => GetPropNameFromLocalPropId(x.CompKey.Level2Key));
            return result;
        }

        public IEnumerable<IPropData> GetValues(IPropBag propBag)
        {
            CheckObjectRef(propBag);
            IEnumerable<IPropData> result = _ourNode.GetPropDataItems(); // .Children.Select(x => x.PropData_Internal);
            return result;
        }

        public IEnumerable<KeyValuePair<PropNameType, IPropData>> GetPropDataItemsWithNames(IPropBag propBag)
        {
            CheckObjectRef(propBag);
            IEnumerable<KeyValuePair<PropNameType, IPropData>> result = _ourNode.GetPropDataItemsWithNames();
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
                        StopWatchingGuest(cKey);
                    }

                    int_propData.SetTypedProp(propertyName, genericTypedProp);

                    if (genericTypedProp.TypedValueAsObject != null && genericTypedProp.TypedValueAsObject.GetType().IsPropBagBased())
                    {
                        // Subscribe to the new value's PropertyChanged event.
                        BeginWatchingGuest(cKey);
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

        public object GetValueFast(IPropBag component, WeakRefKey<PropModelType> propItemSetId, ExKeyT compKey)
        {
            WeakRefKey<IPropBag> propBag_wrKey = new WeakRefKey<IPropBag>(component);
            object result = _propStoreAccessServiceProvider.GetValueFast(propBag_wrKey, propItemSetId, compKey);
            GC.KeepAlive(component);
            return result;
        }

        public bool SetValueFast(IPropBag component, WeakRefKey<PropModelType> propItemSetId, ExKeyT compKey, object value)
        {
            WeakRefKey<IPropBag> propBag_wrKey = new WeakRefKey<IPropBag>(component);
            bool result = _propStoreAccessServiceProvider.SetValueFast(propBag_wrKey, propItemSetId, compKey, value);
            return result;
        }

        #region PropItemSet Management

        public bool IsPropItemSetFixed => _ourNode.IsFixed; // _propStoreAccessServiceProvider.IsPropItemSetFixed(_ourNode);

        public bool TryFixPropItemSet(WeakRefKey<PropModelType> propItemSetId)
        {
            bool result = _propStoreAccessServiceProvider.TryFixPropItemSet(_ourNode, propItemSetId);
            return result;
        }

        public bool TryOpenPropItemSet(/*out object propItemSet_Handle*/)
        {
            if(_propStoreAccessServiceProvider.TryOpenPropItemSet(_ourNode/*, out propItemSet_Handle*/))
            {
                return true;
            }
            else
            {
                //propItemSet_Handle = null;
                return false;
            }
        }

        public bool TryGetPropId(PropNameType propertyName, out PropIdType propId)
        {
            return _ourNode.TryGetPropId(propertyName, out propId);
        }

        public bool TryGetPropName(PropIdType propId, out PropNameType propertyName)
        {
            return _ourNode.TryGetPropertyName(propId, out propertyName);
        }

        public int PropertyCount => _ourNode.PropertyCount;

        public PSAccessServiceInterface CloneProps(IPropBag callingPropBag, IPropBag copySource)
        {
            // Since the caller does not yet have a StoreAccessor (this method is responsble for creating its new StoreAccessor),
            // the caller is using the StoreAccessor that belongs to the copySource to make this call.
            CheckObjectRef(copySource);

            PSAccessServiceInterface newStoreAccessor= CloneProps(callingPropBag, _ourNode.PropNodeCollection);
            return newStoreAccessor;

            //BagNode copySourceStoreNode = _ourNode;

            //PSAccessServiceInterface newStoreAccessor = _propStoreAccessServiceProvider.ClonePSAccessService
            //    (
            //        copySourceStoreNode.PropNodeCollection,
            //        callingPropBag, // The PropBag for which the newStoreAcessor will be built.
            //        out BagNode newStoreNode
            //    );

            //System.Diagnostics.Debug.Assert
            //    (
            //    condition: newStoreAccessor.PropertyCount == this.PropertyCount,
            //    message: "The PropBag clone operation was not completed: The PropItemSet has different contents."
            //    );

            //return newStoreAccessor;
        }

        public PSAccessServiceInterface CloneProps(IPropBag callingPropBag, PropNodeCollectionInterface template)
        {
            CheckTemplate(template);

            PSAccessServiceInterface newStoreAccessor = _propStoreAccessServiceProvider.ClonePSAccessService
                (
                    (PropNodeCollectionIntInterface) template,
                    callingPropBag, // The PropBag for which the newStoreAcessor will be built.
                    out BagNode newStoreNode
                );

            System.Diagnostics.Debug.Assert
                (
                condition: newStoreAccessor.PropertyCount == this.PropertyCount,
                message: "The PropBag clone operation was not completed: The PropItemSet has different contents."
                );

            return newStoreAccessor;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckTemplate(PropNodeCollectionInterface pnc)
        {
            if(!(pnc is PropNodeCollectionIntInterface))
            {
                throw new InvalidOperationException("The template argument must implement the IPropNodeCollection_Internal interface.");
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
            bool wasRemoved = TryRemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        private bool UnregisterHandler<T>(ExKeyT exKey, EventHandler<PcTypedEventArgs<T>> eventHandler)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKey<T>(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            bool wasRemoved = TryRemoveSubscription(subscriptionRequest);
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

            bool wasRemoved = TryRemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        private bool UnregisterHandler(ExKeyT exKey, EventHandler<PcGenEventArgs> eventHandler)
        {
            ISubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(exKey, eventHandler, SubscriptionPriorityGroup.Standard, keepRef: false);
            bool wasRemoved = TryRemoveSubscription(subscriptionRequest);
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

            bool wasRemoved = TryRemoveSubscription(subscriptionRequest);
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

            bool wasRemoved = TryRemoveSubscription(subscriptionRequest);
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

            bool wasRemoved = TryRemoveSubscription(subscriptionRequest);
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

            bool wasRemoved = TryRemoveSubscription(subscriptionRequest);
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

            string msgPrefix;
            string msgSuffix;

            if (subscriptionRequest.HasBeenUsed)
            {
                //System.Diagnostics.Debug.WriteLine($"Created a new Subscription for Property:" +
                //    $"{GetClassAndPropName(subscriptionRequest.OwnerPropId)} ({ subscriptionRequest.OwnerPropId}) / EventType: {result.SubscriptionKind}.");

                msgPrefix = "Created a new Subscription for Property:";
                msgSuffix = "successfully";
                wasAdded = true;
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"The subscription for Property:" +
                //    $" {subscriptionRequest.OwnerPropId} / Event: {result.SubscriptionKind} was not added.");
                msgPrefix = "The subscription for Property:";
                msgSuffix = "was not added";
                wasAdded = false;
            }

            System.Diagnostics.Debug.WriteLine($"{msgPrefix} {GetClassAndPropName(subscriptionRequest.OwnerPropId)} ({ subscriptionRequest.OwnerPropId}) / EventType: {result.SubscriptionKind} {msgSuffix}.");

            return result;
        }

        public bool TryRemoveSubscription(ISubscriptionKeyGen subscriptionRequest)
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
            if (_subscriberCollections != null)
            {
                bool result = _subscriberCollections.TryGetSubscriberCollection(exKey.Level2Key, out subscriberCollection);
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
            if(_subscriberCollections == null)
            {
                _subscriberCollections = new CollectionOfSubscriberCollections();
            }

            SubscriberCollection result = _subscriberCollections.GetOrCreate(exKey.Level2Key, out bool subcriberListWasCreated);
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

        #region Data Source Provider Support

        public DataSourceProvider GetDataSourceProvider(IPropBag propBag, PropIdType propId)
        {
            IManageCViews cViewManager = GetViewManager(propBag, propId);

            if (cViewManager == null) return null;

            return cViewManager.DataSourceProvider;
        }

        // Provides thread-safe, lazy production of a single DataSourceProvider for each PropItem.

        // Get a DataSourceProvider
        public DataSourceProvider GetOrAddDataSourceProvider
            (
            IPropBag propBag,
            PropIdType propId,
            IPropData propData,
            CViewProviderCreator viewBuilder
            )
        {
            IManageCViews CViewManagerGen = GetOrAddViewManager(propBag, propId, propData, viewBuilder);
            DataSourceProvider result = CViewManagerGen.DataSourceProvider;
            return result;
        }

        public IManageCViews GetViewManager(IPropBag propBag, PropIdType propId)
        {
            CheckObjectRef(propBag);

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

        #endregion

        #region Collection View Manager Support

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
            CheckObjectRef(propBag);

            // There is one View Manager for each PropItem. The View Manager for a particular PropItem is created on first use.
            if (_genViewManagers == null)
                _genViewManagers = new ViewManagerCollection();

            IManageCViews result = _genViewManagers.GetOrAdd(propId, CViewGenManagerFactory);
            return result;

            IManageCViews CViewGenManagerFactory(PropIdType propId2)
            {
                Type propertyType = propData.TypedProp.PropTemplate.Type;
                IWatchAPropItemGen propItemWatcherGen = new PropItemWatcherGen(this, propId2, propertyType);

                IProvideADataSourceProvider dSProviderProvider = new PBCollectionDSP_Provider(propData.TypedProp.PropTemplate.PropKind, propItemWatcherGen);
                IManageCViews result2 = new ViewManager(dSProviderProvider, viewBuilder);
                return result2;
            }
        }

        // TODO: Need to make sure the caller is responsible for the life-time of the dsProviderProvider
        // or add a parameter that allows the caller to specify whether or not to dispose the DSP-Provider
        // in its dispose method.
        // ----
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
            CheckObjectRef(propBag);

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
            CViewProviderCreator viewBuilder,            // Method that can be used to create a IProvideAView from a DataSourceProvider.
            BetterLCVCreatorDelegate<TDestination> betterLCVCreatorDelegate
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            CheckObjectRef(propBag);

            // There is one View Manager for each PropItem. The View Manager for a particular PropItem is created on first use.
            if (_genViewManagers == null)
                _genViewManagers = new ViewManagerCollection();

            IManageCViews result = _genViewManagers.GetOrAdd(propId, CViewGenManagerFactory);
            return result;

            IManageCViews CViewGenManagerFactory(PropIdType propId2)
            {
                IProvideADataSourceProvider dSProviderProvider;
                if (propData.TypedProp.PropTemplate.PropKind == PropKindEnum.Prop)
                {
                    IWatchAPropItem<TDal> propItemWatcher = new PropItemWatcher<TDal>(this as PSAccessServiceInternalInterface, propId2);

                    IPropBagMapper<TSource, TDestination> mapper = propBagMapperCreator(mr) as IPropBagMapper<TSource, TDestination>;

                    CrudWithMapping<TDal, TSource, TDestination> mappedDal = 
                        new CrudWithMapping<TDal, TSource, TDestination>(propItemWatcher, mapper);

                    dSProviderProvider = new ClrMappedDSP<TDestination>(mappedDal, betterLCVCreatorDelegate);
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
            CViewProviderCreator viewBuilder,                // Method that can be used to create a IProvideAView from a DataSourceProvider.
            BetterLCVCreatorDelegate<TDestination> betterLCVCreatorDelegate
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            CheckObjectRef(propBag);

            // There is one View Manager for each PropItem. The View Manager for a particular PropItem is created on first use.
            if (_genViewManagers == null)
                _genViewManagers = new ViewManagerCollection();

            IManageCViews result = _genViewManagers.GetOrAdd(propId, CViewGenManagerFactory);
            return result;

            IManageCViews CViewGenManagerFactory(PropIdType propId2)
            {
                IProvideADataSourceProvider dSProviderProvider;
                if (propData.TypedProp.PropTemplate.PropKind == PropKindEnum.Prop)
                {
                    IWatchAPropItem<TDal> propItemWatcher = new PropItemWatcher<TDal>(this as PSAccessServiceInternalInterface, propId2);

                    CrudWithMapping<TDal, TSource, TDestination> mappedDal = 
                        new CrudWithMapping<TDal, TSource, TDestination>(propItemWatcher, mapper);

                    dSProviderProvider = new ClrMappedDSP<TDestination>(mappedDal, betterLCVCreatorDelegate);
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

        #region Collection ViewManager Provider Support

        public IProvideATypedCViewManager<EndEditWrapper<TDestination>, TDestination> GetOrAddViewManagerProviderTyped<TDal, TSource, TDestination>
        (
            IPropBag propBag,   // The client of this service.
            IViewManagerProviderKey viewManagerProviderKey,
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
            PropBagMapperCreator propBagMapperCreator,  // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
            CViewProviderCreator viewBuilder            // Method that can be used to create a IProvideAView from a DataSourceProvider.
        )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
        {
            CheckObjectRef(propBag);

            if (_genViewManagerProviders == null)
                _genViewManagerProviders = new ViewManagerProviderCollection();

            IProvideACViewManager result = _genViewManagerProviders.GetOrAdd(viewManagerProviderKey, CViewGenManagerProviderFactory);
            return result;

            IProvideACViewManager CViewGenManagerProviderFactory(IViewManagerProviderKey key)
            {
                CViewManagerBinder<TDal, TSource, TDestination> result2 =
                    new CViewManagerBinder<TDal, TSource, TDestination>(this, key, propBagMapperCreator, viewBuilder);

                return result2;
            }
        }

        public bool TryGetViewManagerProvider
        (
            IPropBag propBag,   // The client of this service.
            IViewManagerProviderKey viewManagerProviderKey,
            out IProvideACViewManager provideACViewManager
        )
        {
            CheckObjectRef(propBag);

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

        private bool BeginWatchingGuest(ExKeyT propertyKey)
        {
            //TryGetSubscriptions(propertyKey, out SubscriberCollection sc);

            // AGuestPropBagHasANewValue event handler will be called anytime this PropItem's value changes.
            SubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyKey,
                APropItemHostHasANewGuest, SubscriptionPriorityGroup.Internal, keepRef: false);

            AddSubscription(subscriptionRequest, out bool wasAdded);

            //TryGetSubscriptions(propertyKey, out SubscriberCollection sc2);
            return wasAdded;
        }

        private bool StopWatchingGuest(ExKeyT propertyKey)
        {
            SubscriptionKeyGen subscriptionRequest = new SubscriptionKeyGen(propertyKey, 
                APropItemHostHasANewGuest, SubscriptionPriorityGroup.Internal, keepRef: false);

            bool wasRemoved = TryRemoveSubscription(subscriptionRequest);
            return wasRemoved;
        }

        /// <summary>
        /// One of our PropItems hosts a PropBag, and this PropItem's value has changed,
        /// We need to update...
        ///     1. The new guest's parent to point to the hosting PropItem
        ///     2. The old guest's parent to indicate that it has no host PropItem.
        /// </summary>
        /// <param name="sender">The PropBag whose PropItem's value has changed.</param>
        /// <param name="e">The name of the property whose value has changed, the old value and the new value.</param>
        private void APropItemHostHasANewGuest(object sender, PcObjectEventArgs e)
        {
            if (e.NewValueIsUndefined)
            {
                if (e.OldValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.OldValue.GetType().IsPropBagBased(), "The old value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Make old a root.
                    BagNode guestPropBagNode = GetGuestBagNodeFromPropItemVal(e.OldValue);
                    guestPropBagNode.Parent = null;
                }
            }
            else if (e.OldValueIsUndefined)
            {
                if (e.NewValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.NewValue.GetType().IsPropBagBased(), "The new value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Move to child of our property item. This object is currently a root.
                    BagNode guestPropBagNode = GetGuestBagNodeFromPropItemVal(e.NewValue);

                    CheckSenderIsOurPropBag(sender);
                    if (_ourNode.TryGetChild(e.PropertyName, out PropNode propItemNode))
                    {
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
                    BagNode guestPropBagNode = GetGuestBagNodeFromPropItemVal(e.OldValue);
                    guestPropBagNode.Parent = null;
                }

                if (e.NewValue != null)
                {
                    System.Diagnostics.Debug.Assert(e.NewValue.GetType().IsPropBagBased(), "The new value does not implement IPropBag on PropertyChangedWithObjectVals handler in PropStoreAccessService.");

                    // Move to child of our property item. This object is currently a root.
                    BagNode guestPropBagNode = GetGuestBagNodeFromPropItemVal(e.NewValue);

                    CheckSenderIsOurPropBag(sender);
                    if (_ourNode.TryGetChild(e.PropertyName, out PropNode propItemNode))
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
                }
            }
        }

        #endregion

        #region Private Methods

        private BagNode GetGuestBagNodeFromPropItemVal(object propItemValue)
        {
            if (propItemValue is IPropBag propBag)
            {
                if(_propStoreAccessServiceProvider.TryGetPropBagNode(propBag, out BagNode propBagNode))
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


        private BagNode GetPropBagNode(PSAccessServiceInterface propStoreAccessService)
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

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckSenderIsOurPropBag(object sender)
        {
            if(TryGetPropBagForOurNode(out IPropBag ourPropBag))
            {
                if(!ReferenceEquals(sender, ourPropBag))
                {
                    throw new InvalidOperationException("The Sender isn't our PropBag.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not retrieve our PropBag.");
            }
        }

        BagNode _root;
        private BagNode Root
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

        private PropNode GetChild(PropIdType propId)
        {
            if (_ourNode.TryGetChild(propId, out PropNode child))
            {
                PropNode result = child;
                return result;
            }
            else
            {
                throw new KeyNotFoundException("That propId could not be found.");
            }
        }

        private PropNode GetChild(ExKeyT cKey)
        {
            PropNode result = GetChild(cKey.Level2Key);
            return result;
        }

        private PropNode CheckAndGetChild(IPropBag propBag, PropIdType propId, out ExKeyT cKey)
        {
            cKey = GetCompKey(propBag, propId);
            PropNode result = GetChild(propId);
            return result;
        }

        ExKeyT GetCompKey(IPropBag propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            ExKeyT cKey = new SimpleExKey(objectId, propId);

            return cKey;
        }

        //private string GetPropNameFromLocalPropId(PropIdType propId)
        //{
        //    if(TryGetPropName(propId, out PropNameType propertyName))
        //    {
        //        return propertyName;
        //    }
        //    else
        //    {
        //        throw new KeyNotFoundException($"The PropId: {propId} does not correspond with any registered propertyName.");
        //    }
        //}

        /// <summary>
        /// This store accessor holds a reference to a StoreNodeBag; the client IPropBag does not.
        /// Instead of having to lookup the StoreNodeBag that belongs to the client, we can use our reference.
        /// However before doing so we need to make sure that the calling IPropBag does in fact "belong" to this PropStoreAccessService.
        /// </summary>
        /// <param name="propBag"></param>
        /// <returns></returns>
        private void CheckObjectRef(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);
        }

        /// <summary>
        /// This store accessor holds a reference to a StoreNodeBag; the client IPropBag does not.
        /// Instead of having to lookup the StoreNodeBag that belongs to the client, we can use our reference.
        /// However before doing so we need to make sure that the calling IPropBag does in fact "belong" to this PropStoreAccessService.
        /// </summary>
        /// <param name="propBag"></param>
        /// <returns></returns>
        private ObjectIdType GetAndCheckObjectRef(IPropBag propBag)
        {
            if(_clientAccessToken.TryGetTarget(out IPropBag ourPropBag))
            {
                if (!ReferenceEquals(propBag, ourPropBag))
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

        //private void CheckCompKey(ExKeyT cKey)
        //{
        //    if(cKey.Level1Key != _objectId)
        //    {
        //        throw new InvalidOperationException("This PropStoreAccessService can only service the PropBag object that created it.");
        //    }
        //}

        #endregion

        #region Explicit Implementation of the internal interface: IHaveTheStoreNode

        BagNode IHaveTheStoreNode.PropBagNode => _ourNode;

        #endregion

        #region Explicit Implementation of the internal interface: IPropStoreAccessServiceInternal

        bool PSAccessServiceInternalInterface.TryGetChildPropNode(BagNode propBagNode, PropNameType propertyName, out PropNode child)
        {
            bool result = propBagNode.TryGetChild(propertyName, out child);
            if(!result)
            {
                if(TryGetFullClassName(propBagNode, out string fullClassName))
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find property name: {propertyName} on propBagNode with FullClassName: {fullClassName} ({propBagNode.CompKey}).");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find property name: {propertyName} on propBagNode: {propBagNode.CompKey}. (Could not retrieve the full class name.");
                }
            }
            return result;
        }

        // TODO: Provide method that returns the value of StoreNodeProp.PropData_Internal.TypedProp as IProp<T>
        PropNode PSAccessServiceInternalInterface.GetChild(PropIdType propId)
        {
            PropNode result = GetChild(propId);
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

        #endregion

        #region IDisposable Support

        private void ResetAllData()
        {
            if (_subscriberCollections != null)
            {
                // Remove all handlers from each of our Events.
                int numSubsRemoved = 0;
                foreach (SubscriberCollection sc in _subscriberCollections)
                {
                    numSubsRemoved += sc.ClearSubscriptions();
                }

                // Clear the contents of our list of Handlers, one for each property.
                _subscriberCollections.ClearTheListOfSubscriptionPtrs();
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

            if(_genViewManagers != null)
            {
                _genViewManagers.Dispose();
            }

            if (_genViewManagerProviders != null)
            {
                _genViewManagerProviders.Dispose();
            }

            // Dispose each PropItem.
            foreach (PropNode prop in _ourNode.Children)
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

        //public bool PBTestSet(PropBagAbstractBase x)
        //{
        //    x.SetMyValue(-1);

        //    return x.MyValue >= 0;
        //}
    }
}
