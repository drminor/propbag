using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.TypeSafePropertyBag
{
    public interface IListSourceProvider<CT, PT, T> where PT : IETypedProp<CT, T> where CT : IEnumerable<T>
    {
        Func<IETypedProp<CT, T>, ObservableCollection<T>> ObservableCollectionGetter { get; }
        Func<IETypedProp<CT, T>, ReadOnlyObservableCollection<T>> ReadOnlyObservableCollectionGetter { get; }

        Func<IETypedProp<CT, T>, IEnumerable<T>> IEnumerableGetter { get; }

        bool UseObservable { get; } // True to use the ObservableCollectionGetter, False to use the IEnumerable Getter

        bool IsReadOnly { get; } // If True then only the GetTheReadOnlyList should be used. 

        ObservableCollection<T> GetTheList(IETypedProp<CT, T> component);
        ReadOnlyObservableCollection<T> GetTheReadOnlyList(IETypedProp<CT, T> component);
    }
}
