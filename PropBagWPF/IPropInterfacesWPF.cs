using DRM.TypeSafePropertyBag;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    // Collection View Source
    public interface ICViewPropWPF<TCVS, T> : ICViewProp<TCVS, T> where TCVS : CollectionViewSource
    {
        new ObservableCollection<T> Source { get; set; }
    }

    // CollectionViewSource -- ReadOnly 
    public interface IReadOnlyCViewPropWPF<TCVS, T> : IReadOnlyCViewProp<TCVS, T> where TCVS : CollectionViewSource
    {
        new ListCollectionView View { get; }
        new ListCollectionView this[string key] { get; }
        new ObservableCollection<T> GetReadOnlyObservableCollection();
    }

    ///// <summary>
    ///// Adds the IEnumerable<typeparamref name="T"/> interface, the ObservableCollection getter and the
    ///// GetReadOnlyObservableCollection method to the 'standard' CollectionView implementation.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //public interface IOListCollectionView<T> : ICView<T>, ICollectionView, IEditableCollectionViewAddNewItem, IEditableCollectionView, ICollectionViewLiveShaping, IItemProperties, IEnumerable, IComparer, INotifyCollectionChanged, INotifyPropertyChanged
    //{

    //}

}
