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

        new IProvideATypedView<CT, T> GetViewProvider();
        new IProvideATypedView<CT, T> GetViewProvider(string key);
    }

    // Regular View Manager (CollectionViewSource factory)
    public interface IManageCViews
    {
        IList Data { get; }

        //IProvideADataSourceProvider DataSourceProviderProvider { get; }
        DataSourceProvider DataSourceProvider { get; }

        ICollectionView GetDefaultCollectionView();
        ICollectionView GetCollectionView(string key);

        IProvideAView GetViewProvider();
        IProvideAView GetViewProvider(string key);

        void SetDefaultViewProvider(IProvideAView value);
        void SetViewProvider(string key, IProvideAView value);
    }
}
