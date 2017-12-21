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
        public DelegateCache<DoSetDelegate> DoSetDelegateCache { get; }


        public DelegateCache<CreatePropFromStringDelegate> CreatePropFromStringCache { get; }

        public DelegateCache<CreatePropWithNoValueDelegate> CreatePropWithNoValCache { get; }

        public DelegateCache<CreatePropFromObjectDelegate> CreatePropFromObjectCache { get; }

        public TwoTypesDelegateCache<CreateEPropFromStringDelegate> CreateCPropFromStringCache { get; }
        public TwoTypesDelegateCache<CreateEPropFromStringDelegate> CreateCPropFromStringFBCache { get; }

        public TwoTypesDelegateCache<CreateEPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }

        public TwoTypesDelegateCache<CreateEPropFromObjectDelegate> CreateCPropFromObjectCache { get; }

    #endregion

        #region Constructor

        public SimpleDelegateCacheProvider(Type propBagType, Type propCreatorType)
        {
            #region Converter and DoSet

            // TypeDesc<T>-based Converter Cache
            TypeDescBasedTConverterCache = StaticTConverterProvider.TypeDescBasedTConverterCache; // new TypeDescBasedTConverterCache();

            // DoSet 
            MethodInfo doSetMethodInfo = propBagType.GetMethod("DoSetBridge", BindingFlags.Instance | BindingFlags.NonPublic);
            DoSetDelegateCache = new DelegateCache<DoSetDelegate>(doSetMethodInfo);

            #endregion

            #region Prop Creation

            // Create Prop From String
            MethodInfo createPropNoVal_mi = propCreatorType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic);
            CreatePropWithNoValCache = new DelegateCache<CreatePropWithNoValueDelegate>(createPropNoVal_mi);

            // Create Prop With No Value
            MethodInfo createPropFromString_mi = propCreatorType.GetMethod("CreatePropFromString", BindingFlags.Static | BindingFlags.NonPublic);
            CreatePropFromStringCache = new DelegateCache<CreatePropFromStringDelegate>(createPropFromString_mi);

            // Create Prop From Object
            MethodInfo createPropFromObject_mi = propCreatorType.GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic);
            CreatePropFromObjectCache =  new DelegateCache<CreatePropFromObjectDelegate>(createPropFromObject_mi);

            #endregion

            #region Collection Prop Creation

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
        }

        #endregion Constructor
    }
}
