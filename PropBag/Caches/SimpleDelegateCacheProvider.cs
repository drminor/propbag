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
        #region Private Members

        TypeDescBasedTConverterCache _converterCache;

        DelegateCache<DoSetDelegate> _doSetCache;


        DelegateCache<CreatePropFromStringDelegate> _createPropFromStringCache;

        DelegateCache<CreatePropWithNoValueDelegate> _createPropWithNoValCache;

        DelegateCache<CreatePropFromObjectDelegate> _createPropFromObjectCache;

        //DelegateCache<CreateCPropFromStringDelegate> _createCPropFromStringCache;

        //DelegateCache<CreateCPropWithNoValueDelegate> _createCPropWithNoValCache;

        //DelegateCache<CreateCPropFromObjectDelegate> _createCPropFromObjectCache;

        #endregion

        #region Public Properties

        // TypeDesc<T>-based Converter Cache
        public TypeDescBasedTConverterCache TypeDescBasedTConverterCache => _converterCache;

        public DelegateCache<DoSetDelegate> DoSetDelegateCache => _doSetCache;


        public DelegateCache<CreatePropFromStringDelegate> CreatePropFromStringCache => _createPropFromStringCache;

        public DelegateCache<CreatePropWithNoValueDelegate> CreatePropWithNoValCache => _createPropWithNoValCache;

        public DelegateCache<CreatePropFromObjectDelegate> CreatePropFromObjectCache => _createPropFromObjectCache;

        public TwoTypesDelegateCache<CreateCPropFromStringDelegate> CreateCPropFromStringCache { get; }

        public TwoTypesDelegateCache<CreateCPropWithNoValueDelegate> CreateCPropWithNoValCache { get; }

        public TwoTypesDelegateCache<CreateCPropFromObjectDelegate> CreateCPropFromObjectCache { get; }

    #endregion

    #region Constructor

    public SimpleDelegateCacheProvider(Type propBagType, Type propCreatorType)
        {
            #region Converter and DoSet

            // TypeDesc<T>-based Converter Cache
            _converterCache = new TypeDescBasedTConverterCache();

            // DoSet 
            MethodInfo doSetMethodInfo = propBagType.GetMethod("DoSetBridge", BindingFlags.Instance | BindingFlags.NonPublic);
            _doSetCache = new DelegateCache<DoSetDelegate>(doSetMethodInfo);

            #endregion

            #region Prop Creation

            // Create Prop From String
            MethodInfo createPropNoVal_mi = propCreatorType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic);
            _createPropWithNoValCache = new DelegateCache<CreatePropWithNoValueDelegate>(createPropNoVal_mi);


            // Create Prop With No Value
            MethodInfo createPropFromString_mi = propCreatorType.GetMethod("CreatePropFromString", BindingFlags.Static | BindingFlags.NonPublic);
            _createPropFromStringCache = new DelegateCache<CreatePropFromStringDelegate>(createPropFromString_mi);


            // TODO: This is not being used.
            // Create Prop From Object
            MethodInfo createPropFromObject_mi = propCreatorType.GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic);
             _createPropFromObjectCache =  new DelegateCache<CreatePropFromObjectDelegate>(createPropFromObject_mi);

            #endregion

            #region Collection Prop  Creation

            // Create Prop From String
            MethodInfo createCPropNoVal_mi = propCreatorType.GetMethod("CreateCPropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropWithNoValCache = new TwoTypesDelegateCache<CreateCPropWithNoValueDelegate>(createCPropNoVal_mi);


            // Create Prop With No Value
            MethodInfo createCPropFromString_mi = propCreatorType.GetMethod("CreateCPropFromString", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropFromStringCache = new TwoTypesDelegateCache<CreateCPropFromStringDelegate>(createCPropFromString_mi);


            // TODO: This is not being used.
            // Create Prop From Object
            MethodInfo createCPropFromObject_mi = propCreatorType.GetMethod("CreateCPropFromObject", BindingFlags.Static | BindingFlags.NonPublic);
            CreateCPropFromObjectCache = new TwoTypesDelegateCache<CreateCPropFromObjectDelegate>(createCPropFromObject_mi);

            #endregion
        }

        #endregion Constructor
    }
}
