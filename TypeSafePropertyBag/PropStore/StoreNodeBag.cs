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

    using L2KeyManType = IL2KeyMan<UInt32, String>;


    internal class StoreNodeBag : INotifyParentNodeChanged, IDisposable
    {
        #region Private Fields

        //L2KeyManType Level2KeyMan;
        private readonly Dictionary<ExKeyT, StoreNodeProp> _children;

        private ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> _callPSParentNodeChangedEventSubsCache;
        private ParentNCSubscriberCollection _parentNCSubscriberCollection;

        private object _sync = new object();

        #endregion

        #region Events

        // TODO: Make this use WeakReferences to the subscriber.
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
            UnsubscriberForPropStore unsubscriber = new UnsubscriberForPropStore(new WeakReference<StoreNodeBag>(this), subRequest);
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

        private ParentNCSubscriberCollection ParentNCSubscriberCollection
        {
            get
            {
                if(_parentNCSubscriberCollection == null)
                {
                    _parentNCSubscriberCollection = new ParentNCSubscriberCollection();
                }
                return _parentNCSubscriberCollection;
            }
        }

        #region Constructor

        public StoreNodeBag(ExKeyT compKey, IPropBag propBag, L2KeyManType level2KeyMan, ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> callPSParentNodeChangedEventSubsCache)
        {
            CompKey = compKey;
            PropBagProxy = new WeakRefKey<IPropBag>(propBag ?? throw new ArgumentNullException(nameof(propBag)));

            Level2KeyMan = level2KeyMan;

            _callPSParentNodeChangedEventSubsCache = callPSParentNodeChangedEventSubsCache ?? throw new ArgumentNullException(nameof(callPSParentNodeChangedEventSubsCache));
            _parentNCSubscriberCollection = null;

            _children = new Dictionary<ExKeyT, StoreNodeProp>(25);
        }

        #endregion

        #region Public Properties

        // This Composite Key identifies the Object, the PropId portion will always be 0.
        public ExKeyT CompKey { get; }
        public ObjectIdType ObjectId => CompKey.Level1Key;
        public PropIdType PropId => 0;

        public L2KeyManType Level2KeyMan { get; internal set; }

        public WeakRefKey<IPropBag> PropBagProxy { get; }
        public bool IsAlive => PropBagProxy.TryGetTarget(out IPropBag dummy);

        StoreNodeProp _parent;
        public StoreNodeProp Parent
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
                    StoreNodeProp oldValue = _parent;

                    // Use the new value to update this node's parent.
                    _parent = value;

                    // Update the parent to indicate that we are it's new child.
                    if(_parent != null) _parent.Child = this;

                    // Let the subscribers, if any, know about the update.
                    OnParentNodeHasChanged(oldValue, _parent);
                }
            }
        }

        public IEnumerable<StoreNodeProp> Children => _children.Values;

        #endregion

        #region Public Methods

        public bool TryGetPropBag(out IPropBag propBag)
        {
            return PropBagProxy.TryGetTarget(out propBag);
        }

        public bool ChildExists(ExKeyT cKey)
        {
            return _children.ContainsKey(cKey);
        }

        #endregion

        #region Child Accessors


        public int Count
        {
            get
            {
                return _children.Count;
            }
        }

        public void AddChild(StoreNodeProp prop)
        {
            _children.Add(prop.CompKey, prop);
        }

        public bool TryGetChild(PropIdType propId, out StoreNodeProp child)
        {
            ExKeyT cKey = new SimpleExKey(this.CompKey.Level1Key, propId);
            if (_children.TryGetValue(cKey, out child))
            {
                return true;
            }
            else
            {
                child = null;
                return false;
            }
        }

        public bool TryGetChild(ExKeyT cKey, out StoreNodeProp child)
        {
            if (_children.TryGetValue(cKey, out child))
            {
                return true;
            }
            else
            {
                child = null;
                return false;
            }
        }

        public bool TryRemoveChild(ExKeyT cKey, out StoreNodeProp child)
        {
            if (_children.TryGetValue(cKey, out child))
            {
                _children.Remove(cKey);
                return true;
            }
            else
            {
                child = null;
                return false;
            }
        }

        public void RemoveChild(ExKeyT cKey)
        {
            _children.Remove(cKey);
        }

        #endregion

        #region Roots

        public StoreNodeBag Owner
        {
            get
            {
                if (Parent == null) return null;
                return Parent.Parent;
            }
        }

        public IEnumerable<StoreNodeBag> Ancestors
        {
            get
            {
                bool foundOne = false;
                StoreNodeBag lastOwner = Owner;
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

        public StoreNodeBag Root => Ancestors.Last();

        #endregion

        public StoreNodeProp CreateAndAddPropNode(IPropDataInternal propData_Internal, PropNameType propertyName)
        {
            ExKeyT compKey = GetNewCompKey(propertyName);
            StoreNodeProp newPropNode = new StoreNodeProp(compKey, propData_Internal, this);
            return newPropNode;
        }

        private ExKeyT GetNewCompKey(PropNameType propertyName)
        {
            return new SimpleExKey(ObjectId, RegisterNewPropertyName(propertyName));
        }

        #region Level2Key Management

        public bool TryGetPropId(PropNameType propertyName, out PropIdType propId)
        {
            return Level2KeyMan.TryGetFromRaw(propertyName, out propId);
        }

        public bool TryGetPropName(PropIdType propId, out PropNameType propertyName)
        {
            return Level2KeyMan.TryGetFromCooked(propId, out propertyName);
        }

        public PropIdType RegisterNewPropertyName(string propertyName)
        {
            return Level2KeyMan.Add(propertyName);
        }

        public int PropertyCount => Level2KeyMan.PropertyCount;

        #endregion

        #region Raise ParentNodeHasChanged

        // When a StoreNodeBag gets a new host (i.e., a new StoreNodProp parent.)
        private void OnParentNodeHasChanged(StoreNodeProp oldValue, StoreNodeProp newValue)
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
                            catch
                            {
                                System.Diagnostics.Debug.WriteLine("ParentNodeChanged Handler registered with StoreNodeBag, threw an exception.");
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
            foreach (KeyValuePair<ExKeyT, StoreNodeProp> kvp in _children)
            {
                kvp.Value.Dispose();
            }
            _children.Clear();
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    DisposeChildren();

                    if (!Level2KeyMan.IsFixed)
                    {
                        // The Level2KenMan is open (for additions) and therefore it is not shared: It can be disposed.
                        Level2KeyMan.Dispose();
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
