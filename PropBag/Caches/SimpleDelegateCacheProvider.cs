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
    public class SimpleDelegateCacheProvider : IProvideDelegateCaches, IDisposable
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
        //public ICacheDelegatesForTypePair<CreateMappedDSPProviderDelegate> CreateDSPProviderCache { get; }

        public IProvidePropTemplates PropTemplateCache { get; }


        #endregion

        #region Constructor

        public SimpleDelegateCacheProvider(Type propBagType, Type propCreatorType)
        {
            //long startBytes = System.GC.GetTotalMemory(true);
            //long curBytes = startBytes;


            #region Method on PropBag (DoSetDelegate, CVPropFromDsDelegate, and CViewManagerFromDsDelegate)

            // Changed to use Static Method. (DRM 2/6/2018)
            // DoSet (i.e., update) a PropItem's value. 
            MethodInfo doSetMethodInfo = propBagType.GetMethod("DoSetBridge", BindingFlags.Static | BindingFlags.NonPublic);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After GetMethod(DoSetBridge)");

            DoSetDelegateCache = new DelegateCache<DoSetDelegate>(doSetMethodInfo);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After new DelegateCache<DoSetDelegate>");

            // Changed to use Static Method. (DRM 3/16/18)
            // CollectionView Manager using an optional MapperRequest.
            MethodInfo getOrAddCViewManager_mi = propBagType.GetMethod("CViewManagerFromDsBridge", BindingFlags.Static | BindingFlags.NonPublic);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After GetMethod(CViewManagerFromDsBridge)");

            GetOrAddCViewManagerCache = new TwoTypesDelegateCache<CViewManagerFromDsDelegate>(getOrAddCViewManager_mi);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After TwoTypesDelegateCache<CViewManagerFromDsDelegate>");

            // Changed to use Static Method. (DRM 3/16/18)
            // Collection View Manager Provider from a viewManagerProviderKey. Key consists of an optional MapperRequest and a Binding Path.)
            MethodInfo getOrAddCViewManagerProvider_mi = propBagType.GetMethod("CViewManagerProviderFromDsBridge", BindingFlags.Static | BindingFlags.NonPublic);
            GetOrAddCViewManagerProviderCache = new TwoTypesDelegateCache<CViewManagerProviderFromDsDelegate>(getOrAddCViewManagerProvider_mi);

            #endregion

            #region IEnumerable-Type Prop Creation MethodInfo
            #endregion

            #region ObservableCollection<T> Prop Creation MethodInfo

            // TODO: Consolidate these into a single Method, as was done for Scalar Props.

            // Create C Prop with no value
            MethodInfo createCPropNoVal_mi = propCreatorType.GetMethod("CreateCPropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropWithNoValCache = new TwoTypesDelegateCache<CreateCPropWithNoValueDelegate>(createCPropNoVal_mi);

            // Create C Prop From string
            MethodInfo createCPropFromString_mi = propCreatorType.GetMethod("CreateCPropFromString", BindingFlags.Static | BindingFlags.NonPublic);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After GetMethod(CreateCPropFromString)");

            CreateCPropFromStringCache = new TwoTypesDelegateCache<CreateCPropFromStringDelegate>(createCPropFromString_mi);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After new TwoTypesDelegateCache<CreateCPropFromStringDelegate>");

            // Create Prop From Object
            MethodInfo createCPropFromObject_mi = propCreatorType.GetMethod("CreateCPropFromObject", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropFromObjectCache = new TwoTypesDelegateCache<CreateCPropFromObjectDelegate>(createCPropFromObject_mi);

            #endregion

            #region CollectionViewSource Prop Creation MethodInfo

            // NOTE: These don't require a Delegate since they are non-generic methods.
            //// CollectionViewSource
            //MethodInfo createCVSProp_mi = propCreatorType.GetMethod("CreateCVSProp", BindingFlags.Static | BindingFlags.NonPublic);
            //CreateCVSPropCache = new DelegateCache<CreateCVSPropDelegate>(createCVSProp_mi);

            //// CollectionView
            //MethodInfo createCVProp_mi = propCreatorType.GetMethod("CreateCVProp", BindingFlags.Static | BindingFlags.NonPublic);
            //CreateCVPropCache = new DelegateCache<CreateCVPropDelegate>(createCVProp_mi);

            #endregion

            #region DataSource Creation MethodInfo

            //MethodInfo createDSPProvider_mi = propCreatorType.GetMethod("CreateMappedDSPProvider", BindingFlags.Static | BindingFlags.NonPublic);
            //CreateDSPProviderCache = new TwoTypesDelegateCache<CreateMappedDSPProviderDelegate>(createDSPProvider_mi);

            #endregion

            #region Scalar Prop Creation MethodInfo

            // Create Prop 
            MethodInfo createScalarProp_mi = propCreatorType.GetMethod("CreateProp", BindingFlags.Static | BindingFlags.NonPublic);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After GetMethod(CreateProp)");

            CreateScalarPropCache = new DelegateCache<CreateScalarProp>(createScalarProp_mi);
            //curBytes = Sizer.ReportMemConsumption(startBytes, curBytes, "After new DelegateCache<CreatePropFromStringDelegate>");

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

            PropTemplateCache = new SimplePropTemplateCache();

            #endregion
        }

        #endregion Constructor

        #region IDisposable Support

        private void DisposeCaches()
        {
            List<object> dList = new List<object>
            { 
                DoSetDelegateCache,
                GetOrAddCViewManagerCache,
                GetOrAddCViewManagerProviderCache,
                CreateScalarPropCache,
                CreateCPropFromStringCache,
                CreateCPropWithNoValCache,
                CreateCPropWithNoValCache,
                //CreateDSPProviderCache,
                PropTemplateCache
            };

            foreach(object d in dList)
            {
                if(d is IDisposable disable)
                {
                    disable.Dispose();
                }
            }
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    DisposeCaches();

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

    }
}
