using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public class WPFPropFactory : PropFactory
    {
        #region Constructor

        public WPFPropFactory
            (
                IProvideDelegateCaches delegateCacheProvider,
                IConvertValues valueConverter,
                ResolveTypeDelegate typeResolver
            )
            : base(delegateCacheProvider, valueConverter, typeResolver)
        {
        }

        #endregion

        #region Collection-type property creators

        #endregion

        #region Collection View Provider Factory Support

        public override CViewProviderCreator GetCViewProviderFactory()
        {
            return CreateAView;
        }

        private IProvideAView CreateAView(string viewName, DataSourceProvider dataSourceProvider)
        {
            ViewProvider result = new ViewProvider(viewName, dataSourceProvider);
            return result;
        }

        public override BetterLCVCreatorDelegate<T> ListCollectionViewCreator<T>()
        {
            return BetterListCollectionViewCreator<T>.ProduceView;
        }

        #endregion

        #region CollectionViewSource property creators

        public override IProp CreateCVSProp(PropNameType propertyName, IProvideAView viewProvider, IPropTemplate propTemplate) 
        {
            IEqualityComparer<CollectionViewSource> comparer = RefEqualityComparer<CollectionViewSource>.Default;

            bool comparerIsRefEquality = true;

            if (propTemplate == null) propTemplate = GetPropTemplate<CollectionViewSource>(PropKindEnum.CollectionViewSource, PropStorageStrategyEnum.Internal, comparer.Equals, comparerIsRefEquality, null);
            propTemplate.PropCreator = CookedCVSPropCreator;

            ICViewSourceProp<CollectionViewSource> result = new CViewSourceProp(propertyName, viewProvider, (IPropTemplate<CollectionViewSource>) propTemplate);
            return result;
        }

        private static IProp CookedCVSPropCreator(string propertyName2, object initialValue2, bool typeIsSolid2, IPropTemplate propTemplate2)
        {
            ICViewSourceProp<CollectionViewSource> result2 = new CViewSourceProp(propertyName2, (IProvideAView)initialValue2, (IPropTemplate<CollectionViewSource>)propTemplate2);
            return result2;
        }

        public override IProp CreateCVProp(string propertyName, IProvideAView viewProvider, IPropTemplate propTemplate)
        {
            IEqualityComparer<ListCollectionView> comparer = RefEqualityComparer<ListCollectionView>.Default;
            bool comparerIsRefEquality = true;

            if (propTemplate == null) propTemplate = GetPropTemplate<ListCollectionView>(PropKindEnum.CollectionView, PropStorageStrategyEnum.Internal, comparer.Equals, comparerIsRefEquality, null);
            propTemplate.PropCreator = CookedCVPropCreator;

            CViewProp result = new CViewProp(propertyName, viewProvider, (IPropTemplate<ListCollectionView>)propTemplate);
            return result;
        }

        private static IProp CookedCVPropCreator(string propertyName2, object initialValue2, bool typeIsSolid2, IPropTemplate propTemplate2)
        {
            CViewProp result2 = new CViewProp(propertyName2, (IProvideAView)initialValue2, (IPropTemplate<ListCollectionView>)propTemplate2);
            return result2;
        }

        #endregion

        #region DataSource creators

        //public override IMappedDSP<TDestination> CreateMappedDS<TSource, TDestination>
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
        //    IMappedDSP<TDestination> mappedDSP = new IMappedDSP<TDestination>(mappedDal);

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
            //IProvideAutoMappers autoMapperProvider = this._autoMapperProvider ?? throw new InvalidOperationException
            //    ($"This WPFPropFactory instance cannot create IProvideDataSourceProvider instances: No AutoMapperSupport was supplied upon construction.");

            //IPropBagMapperKeyGen realMapperRequest = autoMapperProvider.RegisterMapperRequest(mr.PropModel, mr.SourceType, mr.ConfigPackageName);
            //IPropBagMapperGen genMapper = autoMapperProvider.GetMapper(realMapperRequest);

            IPropBagMapperGen genMapper = null; // GetPropBagMapper(mr, out IPropBagMapperKeyGen mapperRequest);

            Type sourceType = mr.SourceType; // mapperRequest.SourceTypeGenDef.TargetType;
            Type destinationType = mr.PropModel.TargetType; // mapperRequest.DestinationTypeGenDef.TargetType;

            CreateMappedDSPProviderDelegate dspProviderCreator = GetDSPProviderCreator(sourceType, destinationType.BaseType.BaseType);

            IProvideADataSourceProvider result = dspProviderCreator
                (
                this,
                propId,
                propKind,
                iDoCrudDataSource,
                propStoreAccessService,
                genMapper
                );

            return result;
        }

        #endregion
    }
}
