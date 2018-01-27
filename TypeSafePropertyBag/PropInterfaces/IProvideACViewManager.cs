using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvideATypedCViewManager<CT, T> : IProvideACViewManager where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        IManageTypedCViews<CT, T> TypedCViewManager { get; }
    }

    public interface IProvideACViewManager
    {
        IManageCViews CViewManager { get; }
        IViewManagerProviderKey ViewManagerProviderKey { get; }
        event EventHandler<EventArgs> ViewManagerChanged;
    }
}
