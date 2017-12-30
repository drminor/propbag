using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    public delegate IProvideAView CViewProviderCreator(string viewName, DataSourceProvider dataSourceProvider);

    internal delegate IManageCViews CViewManagerCreator(PropIdType sourceCollectionPropId, PropKindEnum propKind, PSAccessServiceInternalInterface propStoreInternalAccessService, CViewProviderCreator cViewProviderCreator);

    public delegate IProvideATypedView<CT, T> TypedCViewProviderCreator<CT, T>(string viewName, DataSourceProvider dataSourceProvider) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

    internal delegate IManageTypedCViews<CT, T> TypedCViewManagerCreator<CT, T>(string viewName, PropIdType sourceCollectionPropId, PropKindEnum propKind, PSAccessServiceInternalInterface propStoreInternalAccessService, TypedCViewProviderCreator<CT,T> typedCViewProviderCreator) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;
}
