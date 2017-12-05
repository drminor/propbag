using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases 
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using L2KeyManType = IL2KeyMan<UInt32, String>;
    #endregion

    internal class StoreNodeBag : INotifyParentNodeChanged
    {
        #region Private Members

        private readonly Dictionary<ExKeyT, StoreNodeProp> _children;

        #endregion

        #region Events

        public event EventHandler<PSNodeParentChangedEventArgs> ParentNodeHasChanged;

        #endregion

        #region Constructor

        public StoreNodeBag(ExKeyT cKey, IPropBagProxy propBagProxy)
        {
            CompKey = cKey;
            PropBagProxy = propBagProxy ?? throw new ArgumentNullException(nameof(propBagProxy));

            _children = new Dictionary<ExKeyT, StoreNodeProp>();
        }

        #endregion

        #region Public Properties

        // This Composite Key identifies the Object, the PropId portion will always be 0.
        public ExKeyT CompKey { get; }

        public ObjectIdType ObjectId => CompKey.Level1Key;
        public PropIdType PropId => 0;

        public IPropBagProxy PropBagProxy { get; }

        StoreNodeProp _parent;
        public StoreNodeProp Parent
        {
            get
            {
                return _parent;
            }

            set
            {
                // remove this from old parent.
                if (_parent != null) _parent.Child = null;

                if (value != _parent)
                {
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

        #endregion

        public IEnumerable<StoreNodeProp> Children => _children.Values;


        public bool ChildExists(ExKeyT cKey)
        {
            return _children.ContainsKey(cKey);
        }

        public void ClearChildren()
        {
            _children.Clear();
        }

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

        #region Raise ParentNodeHasChanged

        private void OnParentNodeHasChanged(StoreNodeProp oldValue, StoreNodeProp newValue)
        {
            Interlocked.CompareExchange(ref ParentNodeHasChanged, null, null)?.Invoke(
                this, new PSNodeParentChangedEventArgs(this.CompKey, oldValue?.CompKey, newValue?.CompKey));

            System.Diagnostics.Debug.WriteLine($"Completed calling OnParentNodeHasChanged. There were {ParentNodeHasChanged?.GetInvocationList()?.Count() ?? 0} subscribers.");
        }

        #endregion
    }
}
