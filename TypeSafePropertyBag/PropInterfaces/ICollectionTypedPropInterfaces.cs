using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    /// <remarks>
    /// Concerning collections:
    /// 
    /// See remarks in file: ICollectionPropInterfaces.cs
    ///
    /// </remarks>

    #region Infrastructure Interfaces

    // Currently this is not used, along with each of the typed versions of the interfaces above.
    public interface ICollectionView<CT, T> where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }

    #endregion

    #region Collection-Type PropItem Interfaces (All derive from IProp.)

    // Collection View
    public interface ICViewTypedProp<CVT, CT, T> : IProp<CVT> where CVT : ICollectionView<CT, T> where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        // TODO: Write an abstract base class for ICollectionView<CT,T>
        IProvideATypedView<CT, T> ViewProvider { get; set; }
    }

    // Observable List / Collection (IList<T> + INotifyCollectionChanged)
    public interface ICProp<CT,T> : IETypedProp<CT, T>, IReadOnlyCProp<CT, T> where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }

    // Observable List / Collection -- ReadOnly
    public interface IReadOnlyCProp<CT,T> : IReadOnlyETypedProp<CT, T> where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }

    // IEnumerable<T> Collection
    public interface IETypedProp<CT, T> : IEProp<CT>, IReadOnlyETypedProp<CT, T>/*, IListSourceProvider<CT,T>*/ where CT : IEnumerable<T>
    {
    }

    // IEnumerable<T> Collection -- ReadOnly
    public interface IReadOnlyETypedProp<CT, T> : IReadOnlyEProp<CT>/*, IListSource*/ where CT : IEnumerable<T>
    {
    }

    #endregion
}
