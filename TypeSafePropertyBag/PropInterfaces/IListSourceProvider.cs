using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    public interface IListSourceProvider<CT, T> where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        Func<ICProp<CT, T>, ObservableCollection<T>> ObservableCollectionGetter { get; }
        Func<IReadOnlyCProp<CT, T>, ReadOnlyObservableCollection<T>> ReadOnlyObservableCollectionGetter { get; }

        Func<ICProp<CT, T>, IEnumerable<T>> IEnumerableGetter { get; }

        bool UseObservable { get; } // True to use the ObservableCollectionGetter, False to use the IEnumerable Getter

        bool IsReadOnly { get; } // If True then only the GetTheReadOnlyList should be used. 

        ObservableCollection<T> GetTheList(ICProp<CT, T> component);
        ReadOnlyObservableCollection<T> GetTheReadOnlyList(ICProp<CT, T> component);
    }
}
