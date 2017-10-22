using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.Collections
{
    public interface IMyListSource<T> : IListSource where T: class
    {
        Func<ObservableCollection<T>> ObservableCollectionGetter { get; }
        Func<IEnumerable<T>> IEnumerableGetter { get; }

        bool UseObservable { get; } // True to use the ObservableCollectionGetter, False to use the IEnumerable Getter

        Action<IPropBag, string> PersistList { get; }
    }
}
