using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeSafePropertyBag;
using System.Reflection;
using ObjectSizeDiagnostics;

namespace DRM.PropBag.Caches
{
    public class SimpleDelegateCacheProvider : IProvideDelegateCaches
    {
        #region Public Properties

        // DoSetDelegate Cache.
        public ICacheDelegates<DoSetDelegate> DoSetDelegateCache { get; }

        // CView
        //public ICacheDelegatesForTypePair<CVPropFromDsDelegate> CreateCViewPropCache { get; }

        // CViewManager
        public ICacheDelegatesForTypePair<CViewManagerFromDsDelegate> GetOrAddCViewManagerCache { get; }
        public ICacheDelegatesForTypePair<CViewManagerProviderFromDsDelegate> GetOrAddCViewManagerProviderCache { get; }


        // Scalar PropItems
        public ICacheDelegates<CreateScalarProp> CreateScalarPropCache { get; }

        //public ICacheDelegates<CreatePropFromStringDelegate> CreateScalarPropCache { get; }
        //public ICacheDelegates<CreatePropWithNoValueDelegate> CreatePropWithNoValCache { get; }
        //public ICacheDelegates<CreatePropFromObjectDelegate> CreatePropFromObjectCache { get; }

        // ObservableCollection<T> PropItems
        public ICacheDelegatesForTypePair<CreateCPropFromStringDelegate> CreateCPropFromStringCache { get; }
        public ICacheDelegatesForTypePair<CreateCPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }
        public ICacheDelegatesForTypePair<CreateCPropFromObjectDelegate> CreateCPropFromObjectCache { get; }

        // DataSourceProviderProvider
        public ICacheDelegatesForTypePair<CreateMappedDSPProviderDelegate> CreateDSPProviderCache { get; }

        public PropTemplateCache PropTemplateCache { get; }


        #endregion

        #region Constructor

        public SimpleDelegateCacheProvider(Type propBagType, Type propCreatorType)
        {
            long startBytes = System.GC.GetTotalMemory(true);
            long curBytes = startBytes;


            #region Method on PropBag (DoSetDelegate, CVPropFromDsDelegate, and CViewManagerFromDsDelegate

            // TypeDesc<T>-based Converter Cache
            //TypeDescBasedTConverterCache = StaticTConverterProvider.TypeDescBasedTConverterCache; // new TypeDescBasedTConverterCache();

            // DoSet 
            MethodInfo doSetMethodInfo = propBagType.GetMethod("DoSetBridge", BindingFlags.Instance | BindingFlags.NonPublic);
            curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After GetMethod(DoSetBridge)");

            DoSetDelegateCache = new DelegateCache<DoSetDelegate>(doSetMethodInfo);
            curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After new DelegateCache<DoSetDelegate>");


            //// AddCollectionViewPropDS using non-generic request and factory
            //MethodInfo addCollectionViewPropDS_mi = propBagType.GetMethod("CVPropFromDsBridge", BindingFlags.Instance | BindingFlags.NonPublic);
            //CreateCViewPropCache = new TwoTypesDelegateCache<CVPropFromDsDelegate>(addCollectionViewPropDS_mi);

            // GetOrAdd CViewManager using non-generic request and factory
            MethodInfo getOrAddCViewManager_mi = propBagType.GetMethod("CViewManagerFromDsBridge", BindingFlags.Instance | BindingFlags.NonPublic);
            curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After GetMethod(CViewManagerFromDsBridge)");

            GetOrAddCViewManagerCache = new TwoTypesDelegateCache<CViewManagerFromDsDelegate>(getOrAddCViewManager_mi);
            curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After TwoTypesDelegateCache<CViewManagerFromDsDelegate>");


            MethodInfo getOrAddCViewManagerProvider_mi = propBagType.GetMethod("CViewManagerProviderFromDsBridge", BindingFlags.Instance | BindingFlags.NonPublic);
            GetOrAddCViewManagerProviderCache = new TwoTypesDelegateCache<CViewManagerProviderFromDsDelegate>(getOrAddCViewManagerProvider_mi);



            #endregion

            #region IEnumerable-Type Prop Creation MethodInfo
            #endregion

            #region ObservableCollection<T> Prop Creation MethodInfo

            // Create C Prop with no value
            MethodInfo createCPropNoVal_mi = propCreatorType.GetMethod("CreateCPropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropWithNoValCache = new TwoTypesDelegateCache<CreateCPropWithNoValueDelegate>(createCPropNoVal_mi);

            // Create C Prop From string
            MethodInfo createCPropFromString_mi = propCreatorType.GetMethod("CreateCPropFromString", BindingFlags.Static | BindingFlags.NonPublic);
            curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After GetMethod(CreateCPropFromString)");

            CreateCPropFromStringCache = new TwoTypesDelegateCache<CreateCPropFromStringDelegate>(createCPropFromString_mi);
            curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After new TwoTypesDelegateCache<CreateCPropFromStringDelegate>");


            // Create Prop From Object
            MethodInfo createCPropFromObject_mi = propCreatorType.GetMethod("CreateCPropFromObject", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropFromObjectCache = new TwoTypesDelegateCache<CreateCPropFromObjectDelegate>(createCPropFromObject_mi);

            #endregion

            #region CollectionViewSource Prop Creation MethodInfo

            //// CollectionViewSource
            //MethodInfo createCVSProp_mi = propCreatorType.GetMethod("CreateCVSProp", BindingFlags.Static | BindingFlags.NonPublic);
            //CreateCVSPropCache = new DelegateCache<CreateCVSPropDelegate>(createCVSProp_mi);

            //// CollectionView
            //MethodInfo createCVProp_mi = propCreatorType.GetMethod("CreateCVProp", BindingFlags.Static | BindingFlags.NonPublic);
            //CreateCVPropCache = new DelegateCache<CreateCVPropDelegate>(createCVProp_mi);

            #endregion

            #region DataSource Creation MethodInfo

            MethodInfo createDSPProvider_mi = propCreatorType.GetMethod("CreateMappedDSPProvider", BindingFlags.Static | BindingFlags.NonPublic);
            CreateDSPProviderCache = new TwoTypesDelegateCache<CreateMappedDSPProviderDelegate>(createDSPProvider_mi);

            #endregion

            #region Scalar Prop Creation MethodInfo

            // Create Prop 
            MethodInfo createScalarProp_mi = propCreatorType.GetMethod("CreateProp", BindingFlags.Static | BindingFlags.NonPublic);
            curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After GetMethod(CreateProp)");

            CreateScalarPropCache = new DelegateCache<CreateScalarProp>(createScalarProp_mi);
            curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After new DelegateCache<CreatePropFromStringDelegate>");

            //// Create Prop From String
            //MethodInfo createPropFromString_mi = propCreatorType.GetMethod("CreatePropFromString", BindingFlags.Static | BindingFlags.NonPublic);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After GetMethod(CreatePropFromString)");

            //CreatePropFromStringCache = new DelegateCache<CreatePropFromStringDelegate>(createPropFromString_mi);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After new DelegateCache<CreatePropFromStringDelegate>");

            //// Create Prop From Object
            //MethodInfo createPropFromObject_mi = propCreatorType.GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic);
            //CreatePropFromObjectCache = new DelegateCache<CreatePropFromObjectDelegate>(createPropFromObject_mi);

            //// Create Prop With No Value
            //MethodInfo createPropNoVal_mi = propCreatorType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic);
            //CreatePropWithNoValCache = new DelegateCache<CreatePropWithNoValueDelegate>(createPropNoVal_mi);

            PropTemplateCache = new PropTemplateCache();

            #endregion
        }

        #endregion Constructor
    }
}
