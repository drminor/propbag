using DRM.TypeSafePropertyBag.Fundamentals.GenericTree;
using System;
using System.Collections.Generic;
using System.Linq;

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
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    #endregion

    internal class PropStoreNode 
    {
        #region Private Members

        private readonly Dictionary<CompositeKeyType, PropStoreNode> _children;

        #endregion

        #region Public Properties

        // Each node holds data about an IPropBag, or a Prop Item belonging to a IPropBag.
        public bool IsObjectNode { get; }

        // If this is an ObjectNode, the PropId portion of the CKey is 0. CKey == ObjectId.
        // If this is an PropNode, the CKey identifies both the IPropBag and the Prop. Its a globally unique PropId.
        public CompositeKeyType CompKey { get; }

        // PropBagProxy will have a value, only for ObjectNodes.
        public IPropBagProxy PropBagProxy { get; }

        // PropData will have a value, only for PropNodes.
        public IPropDataInternal Int_PropData { get; set; }

        public PropStoreNode Parent { get; private set; } 

        public bool IsRoot => Parent == null;

        public Dictionary<CompositeKeyType, PropStoreNode> ChildList => _children;

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> Children => (IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>>)_children;

        #endregion

        #region Constructors

        private PropStoreNode()
        {
            _children = new Dictionary<CompositeKeyType, PropStoreNode>();
        }

        public PropStoreNode(CompositeKeyType ckey, IPropBagProxy propBagProxy) : this()
        {
            IsObjectNode = true;
            CompKey = ckey;
            PropBagProxy = propBagProxy ?? throw new ArgumentNullException(nameof(propBagProxy));
        }

        public PropStoreNode(CompositeKeyType ckey, IPropDataInternal int_PropData) : this()
        {
            IsObjectNode = false;
            CompKey = ckey;
            if(ckey != 0)
            {
                Int_PropData = int_PropData ?? throw new ArgumentNullException(nameof(int_PropData));
            }
            else
            {
                if (int_PropData != null) throw new ArgumentNullException("The CKey is 0, but the int_PropData is not null.");
                Int_PropData = null;
            }
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

        public void AddChild(PropStoreNode child)
        {
            if (child.Parent != null && child.Parent.CompKey != 0)
            {
                throw new ArgumentException($"The child with value [{child}] can not be added because it is not a root node.");
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

        public void MakeItARoot()
        {
            if (IsRoot)
            {
                throw new InvalidOperationException("This node [{this}] is already a root.");
            }
            bool wasRemoved = Parent.ChildList.Remove(this.CompKey);
            Parent = null;
        }

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

        public int Level => Ancestors.Count();

        public PropStoreNode Root => SelfAndAncestors.Last().Value;

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> All => Root.SelfAndDescendants;

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> Ancestors
        {
            get
            {
                if (IsRoot)
                {
                    return Enumerable.Empty<KeyValuePair<CompositeKeyType, PropStoreNode>>();
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

        public IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> SelfAndAncestors => MakeEnumerable(this).Concat(Ancestors);

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

        #region Private Methods

        private bool Other(KeyValuePair<CompositeKeyType, PropStoreNode> kvp) => !ReferenceEquals(kvp.Key, this.CompKey);

        private IEnumerable<KeyValuePair<CompositeKeyType, PropStoreNode>> MakeEnumerable(PropStoreNode node)
        {
            yield return new KeyValuePair<CompositeKeyType, PropStoreNode>(node.CompKey, node);
        }

        #endregion


    }
}
