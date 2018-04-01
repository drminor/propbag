using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag.Unused.GenericTree
{
    public class BaseTreeNode<T> : IAmANodeInATree<T> where T : class
    {
        #region Private Members

        private readonly List<IAmANodeInATree<T>> _children = new List<IAmANodeInATree<T>>();

        #endregion

        #region Public Properties

        public IAmANodeInATree<T> Parent { get; set; } // TODO: see if we can avoid making Set be public.

        public bool IsRoot => Parent == null;

        public List<IAmANodeInATree<T>> ChildList => _children;

        public IEnumerable<IAmANodeInATree<T>> Children => (IEnumerable<IAmANodeInATree<T>>)_children;

        #endregion

        #region Public Methods

        public void AddChild(IAmANodeInATree<T> child, int index = -1)
        {
            if (index < -1)
            {
                throw new ArgumentException("The index can not be lower then -1");
            }
            if (index > _children.Count - 1)
            {
                throw new ArgumentException("The index ({0}) can not be higher then index of the last item. Use the AddChild() method without an index to add at the end".FormatInvariant(index));
            }
            if (!child.IsRoot)
            {
                throw new ArgumentException("The child with value [{0}] can not be added because it is not a root node.".FormatInvariant(child));
            }

            if (Root == child)
            {
                throw new ArgumentException("The child with value [{0}] is the rootnode of the parent.".FormatInvariant(child));
            }

            if (child.SelfAndDescendants.Any(n => this == n))
            {
                throw new ArgumentException("The child with value [{0}] can not be added to itself or its descendants.".FormatInvariant(child));
            }

            child.Parent = this;
            if (index == -1)
            {
                _children.Add(child);
            }
            else
            {
                _children.Insert(index, child);
            }

        }

        public void AddFirstChild(IAmANodeInATree<T> child)
        {
            AddChild(child, 0);
        }

        public void AddFirstSibling(IAmANodeInATree<T> child)
        {
            Parent.AddFirstChild(child);
        }

        public void AddLastSibling(IAmANodeInATree<T> child)
        {
            Parent.AddChild(child);
        }

        public void MakeMeAChildOf(IAmANodeInATree<T> parent)
        {
            if (!IsRoot)
            {
                throw new ArgumentException("This node [{0}] already has a parent".FormatInvariant(parent), "parentNode");
            }
            parent.AddChild(this);
        }

        public void MakeMeARoot()
        {
            if (IsRoot)
            {
                throw new InvalidOperationException("This node is already a root.".FormatInvariant(this));
            }
            Parent.ChildList.Remove(this);
            Parent = null;
        }

        public IEnumerable<IAmANodeInATree<T>> GetItemsAtLevel(int level)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAmANodeInATree<T>> GetNodesAtLevel(int level)
        {
            return Root.GetNodesAtLevelInternal(level);
        }

        public IEnumerable<IAmANodeInATree<T>> GetNodesAtLevelInternal(int level)
        {
            if (level == Level)
            {
                return this.ToIEnumarable();
            }
            return Children.SelectMany(c => c.GetNodesAtLevelInternal(level));
        }

        #endregion

        public int Level => Ancestors.Count();

        public IAmANodeInATree<T> Root => SelfAndAncestors.Last();

        public IEnumerable<IAmANodeInATree<T>> All => Root.SelfAndDescendants;

        public IEnumerable<IAmANodeInATree<T>> Ancestors
        {
            get
            {
                if (IsRoot)
                {
                    return Enumerable.Empty<IAmANodeInATree<T>>();
                }
                else
                {
                    return Parent.ToIEnumarable().Concat(Parent.Ancestors);
                }
            }
        }



        public IEnumerable<IAmANodeInATree<T>> Descendants => SelfAndDescendants.Skip(1);

        public IEnumerable<IAmANodeInATree<T>> SameLevel => SelfAndSameLevel.Where(Other);

        public IEnumerable<IAmANodeInATree<T>> SelfAndAncestors => this.ToIEnumarable().Concat(Ancestors);

        public IEnumerable<IAmANodeInATree<T>> SelfAndChildren => this.ToIEnumarable().Concat(Children);

        public IEnumerable<IAmANodeInATree<T>> SelfAndDescendants => this.ToIEnumarable().Concat(Children.SelectMany(c => c.SelfAndDescendants));

        public IEnumerable<IAmANodeInATree<T>> SelfAndSameLevel => GetNodesAtLevel(Level);

        public IEnumerable<IAmANodeInATree<T>> SelfAndSiblings
        {
            get
            {
                if (IsRoot)
                {
                    return this.ToIEnumarable();
                }
                else
                {
                    return Parent.Children;
                }
            }
        }

        public IEnumerable<IAmANodeInATree<T>> Siblings => SelfAndSiblings.Where(Other);

        #region Private Methods

        private bool Other(IAmANodeInATree<T> node) => !ReferenceEquals(node, this);

        #endregion


    }
}
