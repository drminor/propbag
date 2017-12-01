using DRM.TypeSafePropertyBag.Fundamentals.GenericTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/// <remarks>
/// The logic of providing access to parent and child objects, where each object is the same,
/// was heavily influenced by and much of the code was copied wholesale from:
/// http://www.siepman.nl/blog/post/2013/07/30/tree-node-nodes-descendants-ancestors.aspx
/// http://www.siepman.nl/blog/author/Admin.aspx
/// </remarks>

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases 

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropIdType = UInt32;

    #endregion

    internal class PropStoreNode : IEquatable<PropStoreNode>, INotifyParentNodeChanged
    {
        #region Private Members

        private readonly Dictionary<ExKeyT, PropStoreNode> _children;

        #endregion

        #region Events

        public event EventHandler<PSNodeParentChangedEventArgs> ParentNodeHasChanged;

        #endregion

        #region Public Properties

        // Each node holds data about an IPropBag, or a Prop Item belonging to a IPropBag.
        public bool IsObjectNode { get; }

        // If this is an ObjectNode, the PropId portion of the CKey is 0.
        // If this is an PropNode, the CKey identifies both the IPropBag and the Prop. Its a globally unique PropId.
        public ExKeyT CompKey { get; }

        // PropBagProxy will have a value, only for ObjectNodes.
        public IPropBagProxy PropBagProxy { get; }

        // PropData will have a value, only for PropNodes.
        public IPropDataInternal Int_PropData { get; set; }

        PropStoreNode _parent;
        public PropStoreNode Parent
        {
            get
            {
                if (_parent.IsArtificialRoot) return null;
                return _parent;
            }

            private set
            {
                PropStoreNode oldValue = _parent;
                _parent = value;
                OnParentNodeHasChanged(oldValue, _parent);
            }
        }

        private bool IsArtificialRoot { get; }
        public bool IsRoot => _parent.IsArtificialRoot;

        public Dictionary<ExKeyT, PropStoreNode> ChildList => _children;

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> Children => (IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>>)_children;

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> GetNodesAtLevel(int level)
        {
            return Root.GetNodesAtLevelInternal(level);
        }

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> GetNodesAtLevelInternal(int level)
        {
            if (level == Level)
            {
                return MakeEnumerable(this);
            }
            return Children.SelectMany(c => c.Value.GetNodesAtLevelInternal(level));
        }

        public int Level => -1 + Ancestors.Count();

        public PropStoreNode Root
        {
            get
            {
                if (_parent.IsArtificialRoot)
                {
                    return this;
                }
                else if (IsArtificialRoot)
                {
                    throw new InvalidOperationException("Cannot get the root of the tree from the Artificial Root.");
                }
                else
                {
                    return Ancestors.Last().Value;
                }
            }
        }

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> All
        {
            get
            {
                if(IsArtificialRoot)
                {
                    return Children.SelectMany(c => c.Value.SelfAndDescendants);
                }
                else
                {
                    return SelfAndDescendants;
                }
            }
        }

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> Ancestors
        {
            get
            {
                if (_parent.IsArtificialRoot)
                {
                    // We are the root of this tree, return an empty list.
                    yield return new KeyValuePair<ExKeyT, PropStoreNode>();
                }
                if (IsArtificialRoot)
                {
                    throw new InvalidOperationException("Cannot get the list of ancestors from the Artificial Root.");
                }
                else
                {
                    //List<KeyValuePair<ExKeyT, PropStoreNode>> result = new List<KeyValuePair<ExKeyT, PropStoreNode>>();
                    PropStoreNode lastParent = Parent;
                    while(lastParent != null)
                    {
                        //result.Add(new KeyValuePair<ExKeyT, PropStoreNode>(lastParent.CompKey, lastParent));
                        yield return new KeyValuePair<ExKeyT, PropStoreNode>(lastParent.CompKey, lastParent);
                        lastParent = lastParent.Parent;
                    }
                    //return result as IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>>;
                }
            }
        }

        // TODO: see if we avoid creating the value, just to turn around and skip it.
        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> Descendants => SelfAndDescendants.Skip(1);

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> SameLevel => SelfAndSameLevel.Where(Other);

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> SelfAndAncestors
        {
            get
            {
                if (_parent.IsArtificialRoot)
                {
                    return MakeEnumerable(this);
                }
                else if (IsArtificialRoot)
                {
                    throw new InvalidOperationException("Cannot get the list of SelfAndAncestors from the Artificial Root.");
                }
                else
                {
                    return MakeEnumerable(this).Concat(Ancestors);
                }
            }
        }

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> SelfAndChildren => MakeEnumerable(this).Concat(Children);

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> SelfAndDescendants =>
            MakeEnumerable(this).Concat(Children.SelectMany(c => c.Value.SelfAndDescendants));

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> SelfAndSameLevel => GetNodesAtLevel(Level);

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> SelfAndSiblings
        {
            get
            {
                if (IsRoot)
                {
                    return MakeEnumerable(this);
                }
                else
                {
                    return _parent.Children;
                }
            }
        }

        public IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> Siblings => SelfAndSiblings.Where(Other);

        #endregion

        #region Constructors

        public PropStoreNode()
        {
            CompKey = new SimpleExKey();
            PropBagProxy = null;
            Int_PropData = null;

            IsObjectNode = false;
            IsArtificialRoot = true;
            _children = new Dictionary<ExKeyT, PropStoreNode>();
            Parent = null;
        }

        public PropStoreNode(ExKeyT ckey, IPropBagProxy propBagProxy, PropStoreNode newParent)
        {
            CompKey = ckey;
            PropBagProxy = propBagProxy ?? throw new ArgumentNullException(nameof(propBagProxy));
            Int_PropData = null;

            IsObjectNode = true;

            IsArtificialRoot = false;
            _children = new Dictionary<ExKeyT, PropStoreNode>();

            this._parent = newParent;
            newParent.ChildList.Add(ckey, this);
        }

        public PropStoreNode(ExKeyT ckey, IPropDataInternal int_PropData, PropStoreNode newParent)
        {
            CompKey = ckey;
            PropBagProxy = null;
            Int_PropData = int_PropData ?? throw new ArgumentNullException(nameof(int_PropData));

            IsObjectNode = false;
            IsArtificialRoot = false;
            _children = new Dictionary<ExKeyT, PropStoreNode>();

            this.Parent = newParent;
            newParent.ChildList.Add(ckey, this);
        }

        #endregion

        #region Public Methods

        public PropStoreNode OnlyChildOfPropItem
        {
            get
            {
                if(this.IsObjectNode)
                {
                    throw new InvalidOperationException("OnlyChildOfPropItem can only be called on PropStoreNodes that have IsObjectNode = false.");
                }

                ICollection<KeyValuePair<ExKeyT, PropStoreNode>> childrenAsCollection = _children as ICollection<KeyValuePair<ExKeyT, PropStoreNode>>;
                PropStoreNode result = childrenAsCollection.FirstOrDefault().Value;
                return result;
            }
        }

        public bool TryGetChild(PropIdType propId, out PropStoreNode child)
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

        public bool TryGetChild(ExKeyT cKey, out PropStoreNode child)
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

        public bool TryRemoveChild(ExKeyT cKey, out PropStoreNode child)
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

        private void AddChild(PropStoreNode child)
        {
            if (child.Parent != null)
            {
                throw new ArgumentException($"The child with value [{child}] can not be added because it is already a child of some other node.");
            }

            if (child.SelfAndDescendants.Any(n => this.CompKey == n.Key))
            {
                throw new ArgumentException($"The child with value [{child}] can not be added to itself or its descendants.");
            }

            if (!this.IsArtificialRoot)
            {
                if(child == Root)
                throw new ArgumentException($"The child with value [{child}] is the top-most root node for this tree.");
            }

            if (child.IsObjectNode && this.IsObjectNode) throw new InvalidOperationException("Only PropNodes can be children of an ObjectNode.");
            if (!child.IsObjectNode && !this.IsObjectNode) throw new InvalidOperationException("Only ObjectNodes can be children of a PropNode.");

            if(this.IsObjectNode && this.ChildList.Count > 0)
            {
                throw new InvalidOperationException("PropNodes can only have zero or one children.");
            }

            child.Parent = this;
            _children.Add(child.CompKey, child);
        }

        public void AddSibling(PropStoreNode child)
        {
            _parent.AddChild(child);
        }

        public void MakeItAChildOf(PropStoreNode parent)
        {
            if (!IsRoot)
            {
                throw new ArgumentException("This node [{this}] already has a parent.");
            }
            parent.AddChild(this);
        }

        public void MakeItARoot(PropStoreNode artificialRoot)
        {
            if (IsRoot)
            {
                throw new InvalidOperationException("This node [{this}] is already a root.");
            }
            bool wasRemoved = _parent.ChildList.Remove(this.CompKey);
            if(wasRemoved)
            {
                if(!artificialRoot.IsArtificialRoot)
                {
                    throw new InvalidOperationException("The node provided for the artificial root was not the artficial root.");
                }
                _parent = artificialRoot;
            }
            else
            {
                throw new OperationCanceledException("This node could not be removed from it parent's list of child nodes.");
            }
        }

        #endregion

        #region Private Methods

        private bool Other(KeyValuePair<ExKeyT, PropStoreNode> kvp) => !ReferenceEquals(kvp.Key, this.CompKey);

        private IEnumerable<KeyValuePair<ExKeyT, PropStoreNode>> MakeEnumerable(PropStoreNode node)
        {
            yield return new KeyValuePair<ExKeyT, PropStoreNode>(node.CompKey, node);
        }

        private void OnParentNodeHasChanged(PropStoreNode oldValue, PropStoreNode newValue)
        {
            Interlocked.CompareExchange(ref ParentNodeHasChanged, null, null)?.Invoke(
                this, new PSNodeParentChangedEventArgs(this.CompKey, oldValue.CompKey, newValue.CompKey));

            System.Diagnostics.Debug.WriteLine($"Completed calling OnParentNodeHasChanged. There were {ParentNodeHasChanged?.GetInvocationList()?.Count() ?? 0} subscribers.");
        }

        #endregion

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as PropStoreNode);
        }

        public bool Equals(PropStoreNode other)
        {
            return other != null &&
                   CompKey == other.CompKey &&
                   IsArtificialRoot == other.IsArtificialRoot;
        }

        public override int GetHashCode()
        {
            var hashCode = 990052434;
            hashCode = hashCode * -1521134295 + CompKey.GetHashCode();
            hashCode = hashCode * -1521134295 + IsArtificialRoot.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            string result;

            if (IsObjectNode)
            {
                if (PropBagProxy.PropBagRef.TryGetTarget(out IPropBagInternal target))
                {
                    result = $"{((IPropBag)target).FullClassName} (O:{CompKey.Level1Key}).";
                }
                else
                {
                    result = $"PropStoreNode: {CompKey} for which the Weak Reference holds no object.";
                }
            }
            else
            {
                if (Parent.PropBagProxy.PropBagRef.TryGetTarget(out IPropBagInternal target))
                {
                    if (target.Level2KeyManager.TryGetFromCooked(Int_PropData.PropId, out string propertyName))
                    {
                        result = $"{propertyName} on {Parent} (P:{CompKey.Level2Key}).";
                    }
                    else
                    {
                        result = $"Could not get property name on {Parent}.";
                    }
                }
                else
                {
                    result = $"Child of PropStoreNode: {CompKey} for which the Weak Reference holds no object.";
                }
            }

            return result;
        }

        public static bool operator ==(PropStoreNode node1, PropStoreNode node2)
        {
            return EqualityComparer<PropStoreNode>.Default.Equals(node1, node2);
        }

        public static bool operator !=(PropStoreNode node1, PropStoreNode node2)
        {
            return !(node1 == node2);
        }

        #endregion
    }
}
