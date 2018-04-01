using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.Unused.GenericTree
{
    public interface IAmANodeInATree<T> where T: class
    {
        bool IsRoot { get; }
        int Level { get; }
        IAmANodeInATree<T> Parent { get; set; } // TODO: see if can avoid making set, public.
        IAmANodeInATree<T> Root { get; }

        List<IAmANodeInATree<T>> ChildList { get; } 

        IEnumerable<IAmANodeInATree<T>> All { get; }
        IEnumerable<IAmANodeInATree<T>> Ancestors { get; }
        IEnumerable<IAmANodeInATree<T>> Children { get; }
        IEnumerable<IAmANodeInATree<T>> Descendants { get; }

        IEnumerable<IAmANodeInATree<T>> SameLevel { get; }
        IEnumerable<IAmANodeInATree<T>> SelfAndAncestors { get; }
        IEnumerable<IAmANodeInATree<T>> SelfAndChildren { get; }
        IEnumerable<IAmANodeInATree<T>> SelfAndDescendants { get; }
        IEnumerable<IAmANodeInATree<T>> SelfAndSameLevel { get; }
        IEnumerable<IAmANodeInATree<T>> SelfAndSiblings { get; }
        IEnumerable<IAmANodeInATree<T>> Siblings { get; }

        void AddChild(IAmANodeInATree<T> value, int index = -1);
        void AddFirstChild(IAmANodeInATree<T> value);

        void AddFirstSibling(IAmANodeInATree<T> value);
        void AddLastSibling(IAmANodeInATree<T> value);

        void MakeMeAChildOf(IAmANodeInATree<T> value);

        void MakeMeARoot();
        IEnumerable<IAmANodeInATree<T>> GetItemsAtLevel(int level);
        IEnumerable<IAmANodeInATree<T>> GetNodesAtLevelInternal(int level);
    }
}