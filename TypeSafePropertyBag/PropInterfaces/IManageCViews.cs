using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    // Typed View Manager
    public interface IManageTypedCViews<CT, T> : IManageCViews where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        new CT Data { get; set; }

        new IProvideATypedView<CT, T> GetDefaultViewProvider();
        new IProvideATypedView<CT, T> GetViewProvider(string key);
    }

    // Regular View Manager (CollectionViewSource factory)
    public interface IManageCViews
    {
        IList Data { get; }

        DataSourceProvider DataSourceProvider { get; }

        bool IsDataSourceReadOnly();
        bool IsGetNewItemSupported { get; }

        ICollectionView GetDefaultCollectionView();
        ICollectionView GetCollectionView(string key);

        IProvideAView GetDefaultViewProvider();
        IProvideAView GetViewProvider(string key);

        void SetDefaultViewProvider(IProvideAView value);
        void SetViewProvider(IProvideAView value, string key);

        object GetNewItem();
        void Refresh();
    }
}
