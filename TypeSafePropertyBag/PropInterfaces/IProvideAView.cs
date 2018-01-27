using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    // Typed View Provider
    public interface IProvideATypedView<CT, T> : IProvideAView where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        new ICollectionView<CT, T> View { get; } // A reference to the named view.
    }

    // Regular View Provider
    public interface IProvideAView
    {
        string ViewName { get; }
        ICollectionView View { get; } // A reference to the named view.
        object ViewSource { get; } // A reference to the CollectionViewSource (or similar class.)
        event EventHandler<ViewRefreshedEventArgs> ViewSourceRefreshed;
        DataSourceProvider DataSourceProvider { get; }
    }
}
