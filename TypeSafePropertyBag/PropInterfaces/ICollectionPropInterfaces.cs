
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    /// <remarks>
    /// Concerning collections:
    /// There are two primary types of PropItems:
    /// 1. A read/write collection or a read-only collection of some type (possibly a type that is, or derives from, an IPropBag.)
    /// 2. A ICollectionView 
    /// 
    /// The first type is used to implement regular collections for data persistence.
    /// 
    /// The second type is used to simply binding and to support UI-driven edit operations.
    /// 
    /// The StoreAccessor can, on demand, provide a DataSourceProvider for any PropItem of the first type.
    /// The StoreAccessor can, on demand, also provide a ViewManager for any PropItem of the first type.
    /// 
    /// A ViewManager is given a DataSourceProvider and can produce 
    /// 1.  The default view
    /// 2.  A named view.
    /// 3.  The CollectionViewSource for the default view or any named view.
    /// 4.  A ViewProvider for the default view or any named view.
    /// 
    /// 
    /// The second type, a type that implements the ICollectionView interface, can be assigned a ViewProvider.
    /// A ViewProvider holds a single View and raises the event: ViewRefreshed each time the DataSourceProvider raises its DataChanged event.
    /// 
    /// A ViewProvider is a class that implements IProvideAView
    /// 
    /// 
    /// </remarks>

    #region Collection-Type PropItem Interfaces (All derive from IProp.)

    public interface IUseAViewProvider
    {
        IProvideAView ViewProvider { get; set; }
    }

    // Collection View Source
    public interface ICViewSourceProp<CVST> : /*IProvideADataSourceProvider,*/ IProvideAView, IProp<CVST> where CVST: class 
    {
    }

    // Collection View 
    public interface ICViewProp<CVT> : IProp<CVT>, IProvideAView where CVT : ICollectionView
    {
        IProvideAView ViewProvider { get; set; }
    }

    // Observable List / Collection (IList + INotifyCollectionChanged)
    public interface ICProp<CT> : IEProp<CT>, IReadOnlyCProp<CT> where CT : class, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }

    // Observable List / Collection -- ReadOnly
    public interface IReadOnlyCProp<CT> : IReadOnlyEProp<CT> where CT : class, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }

    // IEnumerable 
    public interface IEProp<CT> : IProp<CT>, IReadOnlyEProp<CT> where CT : IEnumerable
    {
        //void SetListSource(IListSource value);
    }

    // IEnumerable ReadOnly
    public interface IReadOnlyEProp<CT> : IProp<CT> where CT : IEnumerable
    {
        //IList List { get; }
    }

    #endregion
}
