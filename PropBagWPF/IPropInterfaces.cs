using DRM.TypeSafePropertyBag;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace DRM.PropBagWPF
{
    /// <summary>
    /// Adds the IEnumerable<typeparamref name="T"/> interface, the ObservableCollection getter and the
    /// GetReadOnlyObservableCollection method to the 'standard' CollectionView implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOListCollectionView<T> : ICollectionView, IEditableCollectionViewAddNewItem, IEditableCollectionView, ICollectionViewLiveShaping, IItemProperties, IEnumerable<T>, IEnumerable, IComparer, INotifyCollectionChanged, INotifyPropertyChanged
    {
        ObservableCollection<T> ObservableCollection { get; }
        ReadOnlyObservableCollection<T> GetReadOnlyObservableCollection();

        void SetListSource(IListSource value);
        void SetListSource(ObservableCollection<T> source);

    }

    /// <summary>
    /// Provides CollectionViewSource features.
    /// </summary>
    /// <typeparam name="T">The type of each item in the view's list.</typeparam>
    public interface IListCViewProp<CT, T> : ICPropPrivate<CT, T> where CT : IOListCollectionView<T>
    {
        ListCollectionView View { get; }

        ListCollectionView this[string key] { get; }
    }
}
