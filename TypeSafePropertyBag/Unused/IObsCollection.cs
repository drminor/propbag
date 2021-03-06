﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.Unused
{
    public interface IReadOnlyObsCollection<T> :
        IList<T>, ICollection<T>, IEnumerable<T>,
        IList, ICollection, IEnumerable,
        IReadOnlyList<T>, IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }

    public interface IObsCollection<T> : IReadOnlyObsCollection<T>
    {
        void Move(int oldIndex, int newIndex);
    }
}
