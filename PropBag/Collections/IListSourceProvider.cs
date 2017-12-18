using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.Collections
{
    public interface IListSourceProvider<CT,PT,T> where PT : ICPropPrivate<CT,T> where CT : IEnumerable<T>
    {
        Func<ICPropPrivate<CT, T>, ObservableCollection<T>> ObservableCollectionGetter { get; }
        Func<ICPropPrivate<CT, T>, ReadOnlyObservableCollection<T>> ReadOnlyObservableCollectionGetter { get; }

        Func<ICPropPrivate<CT, T>, IEnumerable<T>> IEnumerableGetter { get; }

        bool UseObservable { get; } // True to use the ObservableCollectionGetter, False to use the IEnumerable Getter

        bool IsReadOnly { get; } // If True then only the GetTheReadOnlyList should be used. 

        ObservableCollection<T> GetTheList(ICPropPrivate<CT, T> component);
        ReadOnlyObservableCollection<T> GetTheReadOnlyList(ICPropPrivate<CT, T> component);
    }
}
