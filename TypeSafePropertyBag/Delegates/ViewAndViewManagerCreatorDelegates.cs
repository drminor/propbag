using DRM.TypeSafePropertyBag.DataAccessSupport;
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

    // TODO: Move CollectionView Manager support to a project separate from TypeSafePropertyBag
    // so that the TypeSafePropertyBag does not depend on AutoMapper.

    internal delegate IManageCViews CViewManagerCreator
        (
        PropIdType sourceCollectionPropId,
        PropKindEnum propKind,
        PSAccessServiceInternalInterface propStoreInternalAccessService,
        CViewProviderCreator cViewProviderCreator
        );

    public delegate IProvideATypedView<CT, T> TypedCViewProviderCreator<CT, T>
        (
        string viewName, 
        DataSourceProvider dataSourceProvider
        )
        where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>,
            IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

    internal delegate IManageTypedCViews<CT, T> TypedCViewManagerCreator<CT, T>
        (
        string viewName,
        PropIdType sourceCollectionPropId,
        PropKindEnum propKind,
        PSAccessServiceInternalInterface propStoreInternalAccessService,
        TypedCViewProviderCreator<CT,T> typedCViewProviderCreator
        )
        where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>,
            IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

    public delegate IDoCrudWithMapping<TDestination> CrudWithMappingCreator<TDal, TSource, TDestination>
        (
        IWatchAPropItem<TDal> propItemWatcher
        )
        where TDal : class, IDoCRUD<TSource>
        where TSource : class
        where TDestination : INotifyItemEndEdit;
}
