using System.Collections;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface IReadOnlyObsCollection<T> :
    IList<T>, ICollection<T>, IEnumerable<T>,
    IList, ICollection, IEnumerable,
    IReadOnlyList<T>, IReadOnlyCollection<T>
    {
    }

    public interface IObsCollection<T> : IReadOnlyObsCollection<T>
    {
        void Move(int oldIndex, int newIndex);
    }
}
