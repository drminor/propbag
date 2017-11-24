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

    using CompositeKeyType = UInt64;

    #endregion

    internal class PropStoreNode : IEquatable<PropStoreNode>, INotifyParentNodeChanged
    {
        #region Private Members

        private readonly Dictionary<CompositeKeyType, PropStoreNode> _children;

        #endregion

        #region Events

        public event EventHandler<PSNodeParentChangedEventArgs> ParentNodeHasChanged;

        #endregion

        #region Public Properties

        // Each node holds data about an IPropBag, or a Prop Item belonging to a IPropBag.
        public bool IsObjectNode { get; }

        // If this is an ObjectNode, the PropId portion of the CKey is 0.
        // If this is an PropNode, the CKey identifies both the IPropBag and the Prop. Its a globally unique PropId.
        public CompositeKeyType CompKey { get; }

        // PropBagProxy will have a value, only for ObjectNodes.
        public IPropBagProxy PropBagProxy { get; }

        // PropData will have a value, only for PropNodes.
        public IPropDataInternal Int_PropData { get; set; }

        PropStoreNode _parent;
        public PropStoreNode Parent
        {
            get { return _parent; }
            private set
            {
                PropStoreNode oldValue = _parent;
                _parent = value;
                OnParentNodeHasChanged(oldValue, _parent);
            }
        }

        private bool IsArtificialRoot { get; }
        public bool IsRoot => Parent.IsArtificialRoot;

        public Dictionary<CompositeKeyType, PropStoreNode> ChildList => _children;

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> Children => (IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>>)_children;

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> GetNodesAtLevel(int level)
        {
            return Root.GetNodesAtLevelInternal(level);
        }

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> GetNodesAtLevelInternal(int level)
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
                if (Parent.IsArtificialRoot)
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

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> All => Root.SelfAndDescendants;

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> Ancestors
        {
            get
            {
                if (Parent.IsArtificialRoot)
                {
                    // We are the root of this tree, return an empty list.
                    return Enumerable.Empty<KeyValuePair<CompositeKeyType, PropStoreNode>>();
                }
                if (IsArtificialRoot)
                {
                    throw new InvalidOperationException("Cannot get the list of ancestors from the Artificial Root.");
                }
                else
                {
                    return MakeEnumerable(Parent).Concat(Parent.Ancestors);
                }
            }
        }

        // TODO: see if we avoid creating the value, just to turn around and skip it.
        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> Descendants => SelfAndDescendants.Skip(1);

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> SameLevel => SelfAndSameLevel.Where(Other);

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> SelfAndAncestors
        {
            get
            {
                if (Parent.IsArtificialRoot)
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

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> SelfAndChildren => MakeEnumerable(this).Concat(Children);

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> SelfAndDescendants =>
            MakeEnumerable(this).Concat(Children.SelectMany(c => c.Value.SelfAndDescendants));

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> SelfAndSameLevel => GetNodesAtLevel(Level);

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> SelfAndSiblings
        {
            get
            {
                if (IsRoot)
                {
                    return MakeEnumerable(this);
                }
                else
                {
                    return Parent.Children;
                }
            }
        }

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> Siblings => SelfAndSiblings.Where(Other);

        #endregion

        #region Constructors

        public PropStoreNode()
        {
            CompKey = 0;
            PropBagProxy = null;
            Int_PropData = null;

            IsObjectNode = false;
            IsArtificialRoot = true;
            _children = new Dictionary<CompositeKeyType, PropStoreNode>();
            Parent = null;
        }

        public PropStoreNode(CompositeKeyType ckey, IPropBagProxy propBagProxy, PropStoreNode newParent)
        {
            CompKey = ckey;
            PropBagProxy = propBagProxy ?? throw new ArgumentNullException(nameof(propBagProxy));
            Int_PropData = null;

            IsObjectNode = true;

            IsArtificialRoot = false;
            _children = new Dictionary<CompositeKeyType, PropStoreNode>();

            this.Parent = newParent;
            newParent.ChildList.Add(ckey, this);
        }

        public PropStoreNode(CompositeKeyType ckey, IPropDataInternal int_PropData, PropStoreNode newParent)
        {
            CompKey = ckey;
            PropBagProxy = null;
            Int_PropData = int_PropData ?? throw new ArgumentNullException(nameof(int_PropData));

            IsObjectNode = false;
            IsArtificialRoot = false;
            _children = new Dictionary<CompositeKeyType, PropStoreNode>();

            this.Parent = newParent;
            newParent.ChildList.Add(ckey, this);
        }

        #endregion

        #region Public Methods

        public bool TryGetChild(CompositeKeyType cKey, out PropStoreNode child)
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

        public bool TryRemoveChild(CompositeKeyType cKey, out PropStoreNode child)
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
            // Check to see if the child already has a parent, excluding the "Artifical" root used to hold all "real" roots.
            if (child.Parent != null && child.Parent.CompKey != 0)
            {
                throw new ArgumentException($"The child with value [{child}] can not be added because it is already a child of some other node.");
            }

            if (Root == child)
            {
                throw new ArgumentException($"The child with value [{child}] is the top-most root node for this tree.");
            }

            if (child.SelfAndDescendants.Any(n => this.CompKey == n.Key))
            {
                throw new ArgumentException($"The child with value [{child}] can not be added to itself or its descendants.");
            }

            child.Parent = this;
            _children.Add(child.CompKey, child);
        }

        public void AddSibling(PropStoreNode child)
        {
            Parent.AddChild(child);
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
            bool wasRemoved = Parent.ChildList.Remove(this.CompKey);
            if(wasRemoved)
            {
                if(!artificialRoot.IsArtificialRoot)
                {
                    throw new InvalidOperationException("The node provided for the artificial root was not the artficial root.");
                }
                Parent = artificialRoot;
            }
            else
            {
                throw new OperationCanceledException("This node could not be removed from it parent's list of child nodes.");
            }
        }

        #endregion

        #region Private Methods

        private bool Other(KeyValuePair<CompositeKeyType, PropStoreNode> kvp) => !ReferenceEquals(kvp.Key, this.CompKey);

        private IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> MakeEnumerable(PropStoreNode node)
        {
            yield return new KeyValuePair<CompositeKeyType, PropStoreNode>(node.CompKey, node);
        }

        private void OnParentNodeHasChanged(PropStoreNode oldValue, PropStoreNode newValue)
        {
            Interlocked.CompareExchange(ref ParentNodeHasChanged, null, null)?.Invoke(
                this, new PSNodeParentChangedEventArgs(this.CompKey, oldValue.PropBagProxy, newValue.PropBagProxy));

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
