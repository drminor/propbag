using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public class WPFPropFactory : PropFactory
    {
        #region Private Properties

        IProvideAutoMappers _autoMapperProvider { get; }

        #endregion

        #region Constructor

        public WPFPropFactory
            (
                IProvideDelegateCaches delegateCacheProvider,
                IConvertValues valueConverter,
                ResolveTypeDelegate typeResolver
            )
            : base(delegateCacheProvider, valueConverter, typeResolver)
        {
            _autoMapperProvider = null;
        }

        public WPFPropFactory
            (
                IProvideDelegateCaches delegateCacheProvider,
                IConvertValues valueConverter,
                ResolveTypeDelegate typeResolver,
                IProvideAutoMappers autoMapperProvider
            )
            : base(delegateCacheProvider, valueConverter, typeResolver)
        {
            _autoMapperProvider = autoMapperProvider;
        }

        #endregion

        #region CollectionViewSource property creators

        #endregion

        #region Collection-type property creators

        //public override IETypedProp<CT, T> Create<CT, T>
        //    (
        //    CT initialValue,
        //    string propertyName,
        //    object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal,
        //    bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null
        //    )
        //{
        //    IETypedProp<CT, T> newProp;
        //    if(typeof(CT).IOListCollectionViewBased())
        //    {
        //        IOListCollectionView<T> newVal;
        //        if (initialValue == null)
        //        {
        //            newVal = new OListCollectionView<T>(new ObservableCollection<T>());
        //        }
        //        else
        //        {
        //            newVal = initialValue as IOListCollectionView<T>;
        //        }

        //        // Use reference equality.
        //        Func<IOListCollectionView<T>, IOListCollectionView<T>, bool> newComparer = RefEqualityComparer<IOListCollectionView<T>>.Default.Equals;

        //        // Get the function used to create default values from our ValueConverter.
        //        GetDefaultValueDelegate<IOListCollectionView<T>> defaultValFunc = ValueConverter.GetDefaultValue<IOListCollectionView<T>>;

        //        IListCViewProp<IOListCollectionView<T>, T> fancyNewProp = 
        //            CreateListViewProp<IOListCollectionView<T>, T>(newVal, defaultValFunc,  propertyName, extraInfo, hasStorage, typeIsSolid, newComparer);

        //        newProp = (IETypedProp<CT,T>) fancyNewProp;
        //    }
        //    else
        //    {
        //        newProp = base.Create<CT, T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, comparer);
        //    }

        //    return newProp;
        //}

        //private IListCViewProp<CT,T> CreateListViewProp<CT, T>
        //    (
        //    CT initialValue,
        //    GetDefaultValueDelegate<CT> defaultValFunc,
        //    string propertyName,
        //    object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal,
        //    bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null
        //    ) where CT: IOListCollectionView<T>
        //{
        //    IListCViewProp<CT, T> result = new ListCViewProp<CT, T>(initialValue, defaultValFunc, typeIsSolid, hasStorage, comparer);

        //    return result;
        //}

        #endregion

        #region CollectionViewSource property creators

        public override PropBagMapperCreator GetPropBagMapperFactory()
        {
            return GetPropBagMapper;
        }

        private IPropBagMapperGen GetPropBagMapper(IMapperRequest mr)
        {
            IProvideAutoMappers autoMapperProvider = this._autoMapperProvider ?? throw new InvalidOperationException($"This WPFPropFactory instance cannot create IProvideDataSourceProvider instances: No AutoMapperSupport was supplied upon construction.");

            IPropBagMapperKeyGen realMapperRequest = autoMapperProvider.RegisterMapperRequest(mr.PropModel, mr.SourceType, mr.ConfigPackageName);
            IPropBagMapperGen genMapper = autoMapperProvider.GetMapper(realMapperRequest);
            return genMapper;
        }

        public override CViewProviderCreator GetCViewProviderFactory()
        {
            return CreateAView;
        }

        private IProvideAView CreateAView(string viewName, DataSourceProvider dataSourceProvider)
        {
            ViewProvider result = new ViewProvider(viewName, dataSourceProvider);
            return result;
        }

        public override IProp CreateCVSProp(PropNameType propertyName, IProvideAView viewProvider) 
        {
            ICViewSourceProp<CollectionViewSource> result = new CViewSourceProp(propertyName, viewProvider);
            return (IProp)result;
        }

        public override IProp CreateCVProp(string propertyName, IProvideAView viewProvider)
        {
            //ListCollectionView newVal;
            //if(initialValue == null)
            //{
            //    newVal = null;
            //}
            //else
            //{
            //    if (initialValue is ListCollectionView lcv)
            //    {
            //        newVal = lcv;
            //    }
            //    else
            //    {
            //        throw new ArgumentException($"The initialValue is not a ListCollectionView.", nameof(initialValue));
            //    }
            //}

            CViewProp result = new CViewProp(propertyName, viewProvider);
            return result;
        }

        //public IProp CreateCVPropGen(string propertyName, ICollectionView initialValue)
        //{
        //    CViewProp result = new CViewPropGen(propertyName, initialValue );
        //    return result;
        //}

        #endregion

        #region DataSource creators


        //public override ClrMappedDSP<TDestination> CreateMappedDS<TSource, TDestination>
        //    (
        //    PropIdType propId,
        //    PropKindEnum propKind,
        //    IDoCRUD<TSource> dal,
        //    PSAccessServiceInterface storeAccesor,
        //    IPropBagMapper<TSource, TDestination> mapper  //, out CrudWithMapping<TSource, TDestination> mappedDal
        //    ) 
        //{
        //    // TODO: Create Wrapper around dal, similar to PBCollectionDSP_Provider.cs


        //    // Use the AutoMapper Mapper engine to create a "mapped" IDoCRUD<TDestination> wrapper around the specified instance of the IDoCRUD<TSource>.
        //    CrudWithMapping<IDoCRUD<TSource>, TSource, TDestination> mappedDal =
        //        new CrudWithMapping<IDoCRUD<TSource>, TSource, TDestination>(dal, mapper);

        //    //Create a IProvideADataSourceProvider using the IDoCRUD<TDestination> DataAccessLayer.
        //    ClrMappedDSP<TDestination> mappedDSP = new ClrMappedDSP<TDestination>(mappedDal);

        //    return mappedDSP;
        //}


        #endregion

        #region Property-type property creators

        #endregion

        #region Generic property creators

        // DataSource Provider
        public override IProvideADataSourceProvider GetDSProviderProvider(PropIdType propId, PropKindEnum propKind, 
            object iDoCrudDataSource, PSAccessServiceInterface propStoreAccessService, IMapperRequest mr)
        {
            IProvideAutoMappers autoMapperProvider = this._autoMapperProvider ?? throw new InvalidOperationException($"This WPFPropFactory instance cannot create IProvideDataSourceProvider instances: No AutoMapperSupport was supplied upon construction.");

            IPropBagMapperKeyGen realMapperRequest = autoMapperProvider.RegisterMapperRequest(mr.PropModel, mr.SourceType, mr.ConfigPackageName);
            IPropBagMapperGen genMapper = autoMapperProvider.GetMapper(realMapperRequest);

            Type sourceType = realMapperRequest.SourceTypeGenDef.TargetType;
            Type destinationType = realMapperRequest.DestinationTypeGenDef.TargetType;

            CreateMappedDSPProviderDelegate dspProviderCreator = GetDSPProviderCreator(sourceType, destinationType.BaseType.BaseType);
            IProvideADataSourceProvider result = dspProviderCreator(this, propId, propKind, iDoCrudDataSource, propStoreAccessService, genMapper);

            return result;
        }

        //public override IProp CreateCVSPropFromString(Type typeOfThisProperty, PropNameType propertyName, DataSourceProvider initialValue)
        //{
        //    CreateCVSPropDelegate propCreator = GetCVSPropCreator(typeOfThisProperty);
        //    IProp prop = propCreator(this, propertyName, initialValue);
        //    return prop;
        //}

        //public override IProp CreateCVPropFromString(Type typeofThisProperty, PropNameType propertyName, ICollectionView initialValue)
        //{
        //    CreateCVPropDelegate propCreator = GetCVPropCreator(typeofThisProperty);
        //    IProp prop = propCreator(this, propertyName, initialValue);
        //    return prop;
        //}

        #endregion
    }
}
