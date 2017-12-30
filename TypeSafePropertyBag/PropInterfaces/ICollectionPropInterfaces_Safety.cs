using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    /// <remarks>
    /// Concerning collections: See remarks in file ICollectionPropInterfaces.
    ///
    /// </remarks>

    #region Infrastructure Interfaces


    // Regular View Manager (CollectionViewSource factory)
    public interface IManageCViews<CT> : IProvideADataSourceProvider where CT : IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        CT Data { get; set; }

        ICollectionView GetDefaultCollectionView();
        void SetDefaultCollectionView(ICollectionView value);

        ICollectionView GetCollectionView(string key);
        void SetCollectionView(string key, ICollectionView value);

        object GetCollectionViewSource(string key);
        void SetCollectionViewSource(string key, object collectionViewSource);

        IProvideAView GetViewProvider();
        IProvideAView GetViewProvider(string key);
    }

    //public interface IHaveAView<CT, T> : IHaveAView where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    //{
    //    new CT SourceCollection { get; } // The original collection
    //    new ICollectionView<CT, T> View { get; } // The default View for this collection-type PropItem.
    //    new ICollectionView<CT, T> this[string key] { get; } // A named View of this collection-type PropItem. 
    //}

    //public interface IHaveAView
    //{
    //    object SourceCollection { get; } // The original collection
    //    ICollectionView View { get; } // The default View for this collection-type PropItem.
    //    ICollectionView this[string key] { get; } // A named View of this collection-type PropItem.

    //    event EventHandler<ViewRefreshedEventArgs> ViewRefreshed;
    //}
    
    public interface IProvideAView
    {
        string ViewName { get; }
        ICollectionView View { get; } // A reference to the named view.
        event EventHandler<ViewRefreshedEventArgs> ViewRefreshed;
    }

    #endregion

    #region Collection-Type PropItem Interfaces (All derive from IProp.)

    // Collection View Source
    public interface ICViewSourceProp<CVST> : IProvideADataSourceProvider, IProvideAView, IProp<CVST> where CVST: class 
    {

    }


    // Collection View Gen(No ReadOnly partner, since ICollectionView is inherently read-only.
    public interface ICViewProp<CVT>: IProp<CVT> where CVT : ICollectionView
    {
        IProvideAView ViewProvider { get; set; }
    }



    // IList<T> + INotifyCollectionChanged -- ReadOnly
    public interface IReadOnlyCProp<CT,T> : IReadOnlyETypedProp<CT, T> where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {

    }

    


    // IEnumerable Collections
    public interface IEProp<CT> : IProp<CT>, IReadOnlyEProp<CT> where CT : IEnumerable
    {
        //void SetListSource(IListSource value);
    }

    // IEnumerable Collections -- ReadOnly
    public interface IReadOnlyEProp<CT> : IProp<CT> where CT : IEnumerable
    {
        //IList List { get; }
    }

    #endregion
}
