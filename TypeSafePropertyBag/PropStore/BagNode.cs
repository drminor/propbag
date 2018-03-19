using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropModelType = IPropModel<String>;
    using PropItemSetKeyType = PropItemSetKey<String>;

    using PropNodeCollectionIntInterface = IPropNodeCollection_Internal<UInt32, String>;

    internal class BagNode : INotifyParentNodeChanged, IDisposable
    {
        #region Private Fields

        private PropNodeCollectionIntInterface _propNodeCollection;

        private ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> _callPSParentNodeChangedEventSubsCache;

        private ParentNCSubscriberCollection _parentNCSubscriberCollection;
        private ParentNCSubscriberCollection ParentNCSubscriberCollection
        {
            get
            {
                if (_parentNCSubscriberCollection == null)
                {
                    _parentNCSubscriberCollection = new ParentNCSubscriberCollection();
                }
                return _parentNCSubscriberCollection;
            }
        }

        private object _sync = new object();

        #endregion

        #region Events

        public event EventHandler<PSNodeParentChangedEventArgs> ParentNodeHasChanged
        {
            add
            {
                ParentNCSubscriptionRequest subRequest = new ParentNCSubscriptionRequest(CompKey, value);
                lock (_sync)
                {
                    ParentNCSubscription sub = ParentNCSubscriberCollection.GetOrAdd(subRequest, _callPSParentNodeChangedEventSubsCache);
                }
            }
            remove
            {
                ParentNCSubscriptionRequest subRequest = new ParentNCSubscriptionRequest(CompKey, value);
                lock (_sync)
                {
                    if (!ParentNCSubscriberCollection.TryRemoveSubscription(subRequest))
                    {
                        // TODO: Make this better.
                        System.Diagnostics.Debug.WriteLine("Could not remove ParentNodeChanged subsription.");
                    }
                }
            }
        }

        public IDisposable SubscribeToParentNodeHasChanged(EventHandler<PSNodeParentChangedEventArgs> handler)
        {
            ParentNCSubscriptionRequest subRequest = new ParentNCSubscriptionRequest(CompKey, handler);
            lock (_sync)
            {
                ParentNCSubscription sub = ParentNCSubscriberCollection.GetOrAdd(subRequest, _callPSParentNodeChangedEventSubsCache);
            }
            UnsubscriberForPropStore unsubscriber = new UnsubscriberForPropStore(new WeakReference<BagNode>(this), subRequest);
            return unsubscriber;
        }

        public bool UnsubscribeToParentNodeHasChanged(EventHandler<PSNodeParentChangedEventArgs> handler)
        {
            ParentNCSubscriptionRequest subRequest = new ParentNCSubscriptionRequest(CompKey, handler);
            return UnsubscribeToParentNodeHasChanged(subRequest);
        }

        public bool UnsubscribeToParentNodeHasChanged(ParentNCSubscriptionRequest subRequest)
        {
            lock (_sync)
            {
                if (ParentNCSubscriberCollection.TryRemoveSubscription(subRequest))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Constructor

        public BagNode(ObjectIdType objectId, IPropBag propBag, int maxPropsPerObject, ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> callPSParentNodeChangedEventSubsCache)
            : this(objectId, propBag, null, maxPropsPerObject, callPSParentNodeChangedEventSubsCache)
        {
        }

        /// <summary>
        /// Creates a new BagNode for the specified PropBag, optionally using a collection of PropNodes as the source.
        /// The source PropNodes are cloned. If the value of a PropNode cannot be cloned, an InvalidOperation exception will be thrown.
        /// This is the core of IPropBag.Clone()
        /// </summary>
        /// <param name="objectId">The new globally unique Id to use for this new BagNode.</param>
        /// <param name="propBag">The client IPropBag.</param>
        /// <param name="template">A Collection of PropNodes to use as a template for the new PropBag's Child PropItems. Can be null.</param>
        /// <param name="maxPropsPerObject">The maximum number of PropItems a single PropBag can have.</param>
        /// <param name="callPSParentNodeChangedEventSubsCache">A reference to a service that caches Parent Node Change Event dispatchers.</param>
        public BagNode(ObjectIdType objectId, IPropBag propBag, PropNodeCollectionIntInterface template, int maxPropsPerObject, ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> callPSParentNodeChangedEventSubsCache)
        {

            CompKey = new SimpleExKey(objectId, 0);
            PropBagProxy = new WeakRefKey<IPropBag>(propBag ?? throw new ArgumentNullException(nameof(propBag)));

            _callPSParentNodeChangedEventSubsCache = callPSParentNodeChangedEventSubsCache ?? throw new ArgumentNullException(nameof(callPSParentNodeChangedEventSubsCache));
            _parentNCSubscriberCollection = null;

            if (template == null)
            {
                _propNodeCollection = new PropNodeCollection(maxPropsPerObject);
            }
            else
            {
                _propNodeCollection = ClonePropNodes(template, this);

                //var x = EqualityComparer<WeakRefKey<PropModelType>?>.Default;
                //bool propItemSetIdsMatch = x.Equals(_propNodeCollection.PropItemSetId, template.PropItemSetId);
                //System.Diagnostics.Debug.Assert(EqualityComparer<WeakRefKey<PropModelType>?>.Default.Equals(_propNodeCollection.PropItemSetId, template.PropItemSetId), "PropItemSetIds don't match.");

                bool propItemSetIdsMatch = _propNodeCollection.PropItemSetKey == template.PropItemSetKey;

                System.Diagnostics.Debug.Assert(propItemSetIdsMatch, "PropItemSetIds don't match.");
                System.Diagnostics.Debug.Assert(_propNodeCollection.IsFixed == template.IsFixed, "IsFixed doesn't match.");
            }
        }

        private PropNodeCollectionIntInterface ClonePropNodes(PropNodeCollectionIntInterface sourcePNC, BagNode targetParent)
        {
            PropNodeCollectionIntInterface result;

            List<PropNode> temp = new List<PropNode>();
            foreach (PropNode propNode in sourcePNC.GetPropNodes())
            {
                PropNode newPropNode = propNode.CloneForNewParent(targetParent, useExistingValues: true);
                temp.Add(newPropNode);
            }

            if (sourcePNC.IsFixed)
            {
                System.Diagnostics.Debug.Assert(!sourcePNC.PropItemSetKey.IsEmpty, "We found a fixed PropSetCollection that has an empty PropItemSetKey.");
                // Create a Fixed PropNodeCollection.
                result = new PropNodeCollectionFixed(temp, sourcePNC.PropItemSetKey, sourcePNC.MaxPropsPerObject);
            }
            else
            {
                // Create an open PropNodeCollection.
                result = new PropNodeCollection(temp, sourcePNC.PropItemSetKey, sourcePNC.MaxPropsPerObject);
            }

            return result;
        }

        #endregion

        #region Public Properties

        // This Composite Key identifies the Object, the PropId portion will always be 0.
        public ExKeyT CompKey { get; }

        public ObjectIdType ObjectId => CompKey.Level1Key;
        public PropIdType PropId => 0;

        public WeakRefKey<IPropBag> PropBagProxy { get; }
        public bool IsAlive => PropBagProxy.TryGetTarget(out IPropBag dummy);

        PropNode _parent;
        public PropNode Parent
        {
            get
            {
                return _parent;
            }

            set
            {
                if (value != _parent)
                {
                    // remove this from old parent.
                    if (_parent != null) _parent.Child = null;

                    // save old value to use when raising OnParentNodeHasChanged event.
                    PropNode oldValue = _parent;

                    // Use the new value to update this node's parent.
                    _parent = value;

                    // Update the parent to indicate that we are it's new child.
                    if(_parent != null) _parent.Child = this;

                    // Let the subscribers, if any, know about the update.
                    OnParentNodeHasChanged(oldValue, _parent);
                }
            }
        }

        public IEnumerable<PropNode> Children => _propNodeCollection.GetPropNodes();

        public bool IsFixed => _propNodeCollection.IsFixed;

        public PropNodeCollectionIntInterface PropNodeCollection
        {
            get
            {
                return _propNodeCollection;
            }
            set
            {
                lock(_sync)
                {
                    _propNodeCollection = value;
                }
            }
        }

        //public WeakRefKey<PropModelType>? PropItemSetId => _propNodeCollection.PropItemSetKey;
        public PropItemSetKeyType PropItemSetKey => _propNodeCollection.PropItemSetKey;

        #endregion

        #region Public Methods

        public bool TryGetPropBag(out IPropBag propBag)
        {
            return PropBagProxy.TryGetTarget(out propBag);
        }

        public PropNode CreateAndAddPropNode(IPropDataInternal propData_Internal, PropNameType propertyName)
        {
            if(_propNodeCollection.IsFixed)
            {
                
            }

            PropNode newPropNode = _propNodeCollection.CreateAndAdd(propData_Internal, propertyName, this);
            return newPropNode;
        }

        //public void AddChild(PropNode prop)
        //{
        //    _propNodeCollection.Add(prop.PropId, prop);
        //}

        public bool TryGetChild(PropIdType propId, out PropNode child)
        {
            if (_propNodeCollection.TryGetPropNode(propId, out child))
            {
                return true;
            }
            else
            {
                child = null;
                return false;
            }
        }

        public bool TryGetChild(PropNameType propertyName, out PropNode child)
        {
            if (_propNodeCollection.TryGetPropNode(propertyName, out child))
            {
                return true;
            }
            else
            {
                child = null;
                return false;
            }
        }

        //public bool TryGetChild(ExKeyT cKey, out PropNode child)
        //{
        //    if (_propNodeCollection.TryGetPropNode(cKey.Level2Key, out child))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        child = null;
        //        return false;
        //    }
        //}

        public bool TryRemoveChild(PropIdType propId, out PropNode child)
        {
            bool result = _propNodeCollection.TryRemove(propId, out child);
            return result;
        }


        public bool TryGetPropId(PropNameType propertyName, out PropIdType propId)
        {
            return _propNodeCollection.TryGetPropId(propertyName, out propId);
        }

        public bool TryGetPropertyName(PropIdType propId, out PropNameType propertyName)
        {
            return _propNodeCollection.TryGetPropertyName(propId, out propertyName);
        }

        public bool ChildExists(PropIdType propId)
        {
            bool result = _propNodeCollection.Contains(propId);
            return result;
        }

        public int PropertyCount
        {
            get
            {
                return _propNodeCollection.Count;
            }
        }

        public IEnumerable<PropIdType> GetPropIds()
        {
            IEnumerable<PropIdType> result = _propNodeCollection.GetPropIds();
            return result;
        }

        public IEnumerable<PropNameType> GetPropNames()
        {
            IEnumerable<PropNameType> result = _propNodeCollection.GetPropNames();
            return result;
        }

        public IEnumerable<PropNode> GetPropNodes()
        {
            IEnumerable<PropNode> result = _propNodeCollection.GetPropNodes();
            return result;
        }

        public IEnumerable<IPropData> GetPropDataItems()
        {
            IEnumerable<IPropDataInternal> result = _propNodeCollection.GetPropDataItems();
            return result;
        }

        public IEnumerable<KeyValuePair<PropNameType, IPropData>> GetPropDataItemsWithNames()
        {
            IEnumerable<KeyValuePair<PropNameType, IPropData>> result = _propNodeCollection.GetPropDataItemsDict();
            return result;
        }

        public IReadOnlyDictionary<PropNameType, IPropData> GetPropDataItemsDict()
        {
            IReadOnlyDictionary<PropNameType, IPropData> result = _propNodeCollection.GetPropDataItemsDict();
            return result;
        }

        #endregion

        #region Roots

        public BagNode Owner
        {
            get
            {
                if (Parent == null) return null;
                return Parent.Parent;
            }
        }

        public IEnumerable<BagNode> Ancestors
        {
            get
            {
                bool foundOne = false;
                BagNode lastOwner = Owner;
                while (lastOwner != null)
                {
                    foundOne = true;
                    yield return lastOwner;
                    lastOwner = lastOwner.Owner;
                }
                if(!foundOne)
                {
                    yield return null;
                }
            }
        }

        public BagNode Root => Ancestors.Last();

        #endregion

        #region Raise ParentNodeHasChanged

        // When a BagNode gets a new host (i.e., a new PropNode parent.)
        private void OnParentNodeHasChanged(PropNode oldValue, PropNode newValue)
        {
            List<ParentNCSubscription> subs = null;
            int numberOfSubscribers = 0;
            int numDispatched = 0;
            lock (_sync)
            {
                if(_parentNCSubscriberCollection != null)
                {
                    ExKeyT oldNodeKey = oldValue?.CompKey ?? new SimpleExKey();
                    ExKeyT newNodeKey = newValue?.CompKey ?? new SimpleExKey();
                    subs = _parentNCSubscriberCollection.ToList();
                    foreach (ParentNCSubscription sub in subs)
                    {
                        numberOfSubscribers++;
                        object target = sub.Target.Target;
                        if(target != null)
                        {
                            try
                            {
                                sub.Dispatcher(target, this, new PSNodeParentChangedEventArgs(this.CompKey, oldNodeKey, newNodeKey), sub.Proxy);
                                numDispatched++;
                            }
                            catch (InvalidOperationException ioe)
                            {
                                System.Diagnostics.Debug.WriteLine($"ParentNodeChanged Handler registered with StoreNodeBag, threw an exception: {ioe.Message}");
                                if(ioe.InnerException != null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"The inner exception is {ioe.InnerException.Message}");
                                    if(ioe.InnerException.InnerException != null)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"And that exception's inner exception is {ioe.InnerException.InnerException.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Completed calling OnParentNodeHasChanged. There were {numberOfSubscribers} subscribers, of which {numDispatched} were dispatched.");
        }

        #endregion

        #region IDisposable Support

        private void DisposeChildren()
        {
            foreach (PropNode propNode in _propNodeCollection.GetPropNodes())
            {
                propNode.Dispose();
            }
            _propNodeCollection.Clear();
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    if (!_propNodeCollection.IsFixed)
                    {
                        // The PropNodeCollection is open (for additions) and therefore it is not shared: It can be disposed.
                        DisposeChildren();
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

        public override string ToString()
        {
            return CompKey.ToString();
        }

        #endregion
    }
}
