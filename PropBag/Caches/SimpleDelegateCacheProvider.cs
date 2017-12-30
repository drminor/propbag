using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeSafePropertyBag;
using System.Reflection;

namespace DRM.PropBag.Caches
{
    public class SimpleDelegateCacheProvider : IProvideDelegateCaches
    {
        #region Public Properties

        // TypeDesc<T>-based Converter Cache
        public TypeDescBasedTConverterCache TypeDescBasedTConverterCache { get; }

        // DoSetDelegate Cache.
        public ICacheDelegates<DoSetDelegate> DoSetDelegateCache { get; }


        public ICacheDelegates<CreatePropFromStringDelegate> CreatePropFromStringCache { get; }

        public ICacheDelegates<CreatePropWithNoValueDelegate> CreatePropWithNoValCache { get; }

        public ICacheDelegates<CreatePropFromObjectDelegate> CreatePropFromObjectCache { get; }

        public ICacheDelegatesForTypePair<CreateEPropFromStringDelegate> CreateCPropFromStringCache { get; }
        public ICacheDelegatesForTypePair<CreateEPropFromStringDelegate> CreateCPropFromStringFBCache { get; }

        public ICacheDelegatesForTypePair<CreateEPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }

        public ICacheDelegatesForTypePair<CreateEPropFromObjectDelegate> CreateCPropFromObjectCache { get; }

        //public ICacheDelegates<CreateCVSPropDelegate> CreateCVSPropCache { get; }

        //public ICacheDelegates<CreateCVPropDelegate> CreateCVPropCache { get; }


        #endregion

        #region Constructor

        public SimpleDelegateCacheProvider(Type propBagType, Type propCreatorType)
        {
            #region Converter and DoSet MethodInfo

            // TypeDesc<T>-based Converter Cache
            TypeDescBasedTConverterCache = StaticTConverterProvider.TypeDescBasedTConverterCache; // new TypeDescBasedTConverterCache();

            // DoSet 
            MethodInfo doSetMethodInfo = propBagType.GetMethod("DoSetBridge", BindingFlags.Instance | BindingFlags.NonPublic);
            DoSetDelegateCache = new DelegateCache<DoSetDelegate>(doSetMethodInfo);

            #endregion

            #region IEnumerable-Type Prop Creation MethodInfo
            #endregion

            #region IObsCollection<T> and ObservableCollection<T> Prop Creation MethodInfo

            // Create C Prop with no value
            MethodInfo createCPropNoVal_mi = propCreatorType.GetMethod("CreateEPropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropWithNoValCache = new TwoTypesDelegateCache<CreateEPropWithNoValueDelegate>(createCPropNoVal_mi);

            // Create C Prop From string
            MethodInfo createCPropFromString_mi = propCreatorType.GetMethod("CreateEPropFromString", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropFromStringCache = new TwoTypesDelegateCache<CreateEPropFromStringDelegate>(createCPropFromString_mi);

            // Create C Prop From string FALL BACK to ObservableCollection
            MethodInfo createCPropFromStringFB_mi = propCreatorType.GetMethod("CreateEPropFromStringFB", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropFromStringFBCache = new TwoTypesDelegateCache<CreateEPropFromStringDelegate>(createCPropFromStringFB_mi);

            // Create Prop From Object
            MethodInfo createCPropFromObject_mi = propCreatorType.GetMethod("CreateEPropFromObject", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropFromObjectCache = new TwoTypesDelegateCache<CreateEPropFromObjectDelegate>(createCPropFromObject_mi);

            #endregion

            #region CollectionViewSource Prop Creation MethodInfo

            //// CollectionViewSource
            //MethodInfo createCVSProp_mi = propCreatorType.GetMethod("CreateCVSProp", BindingFlags.Static | BindingFlags.NonPublic);
            //CreateCVSPropCache = new DelegateCache<CreateCVSPropDelegate>(createCVSProp_mi);

            //// CollectionView
            //MethodInfo createCVProp_mi = propCreatorType.GetMethod("CreateCVProp", BindingFlags.Static | BindingFlags.NonPublic);
            //CreateCVPropCache = new DelegateCache<CreateCVPropDelegate>(createCVProp_mi);

            #endregion

            #region Scalar Prop Creation MethodInfo

            // Create Prop From String
            MethodInfo createPropNoVal_mi = propCreatorType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic);
            CreatePropWithNoValCache = new DelegateCache<CreatePropWithNoValueDelegate>(createPropNoVal_mi);

            // Create Prop With No Value
            MethodInfo createPropFromString_mi = propCreatorType.GetMethod("CreatePropFromString", BindingFlags.Static | BindingFlags.NonPublic);
            CreatePropFromStringCache = new DelegateCache<CreatePropFromStringDelegate>(createPropFromString_mi);

            // Create Prop From Object
            MethodInfo createPropFromObject_mi = propCreatorType.GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic);
            CreatePropFromObjectCache = new DelegateCache<CreatePropFromObjectDelegate>(createPropFromObject_mi);

            #endregion
        }

        #endregion Constructor
    }
}
