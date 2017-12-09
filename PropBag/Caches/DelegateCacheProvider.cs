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
    public class DelegateCacheProvider<T> where T: IPropBag
    {
        #region Private Members

        Lazy<TypeDescBasedTConverterCache> _converterCache;

        Lazy<DelegateCache<DoSetDelegate>> _doSetCache;

        Lazy<DelegateCache<CallPcObjEventSubscriberDelegate>> _callPcObjEventSubsCache;

        Lazy<DelegateCache<CallPcGenEventSubscriberDelegate>> _callPcGenEventSubsCache;

        Lazy<DelegateCache<CallPcTypedEventSubscriberDelegate>> _callPcTypeEventSubsCache;

        Lazy<DelegateCache<CallPcStandardEventSubscriberDelegate>> _callPcStEventSubsCache;

        Lazy<DelegateCache<CallPChangingEventSubscriberDelegate>> _callPChangingEventSubsCache;


        Lazy<DelegateCache<CreatePropFromStringDelegate>> _createPropFromStringCache;

        Lazy<DelegateCache<CreatePropWithNoValueDelegate>> _createPropWithNoValCache;

        Lazy<DelegateCache<CreatePropFromObjectDelegate>> _createPropFromObjectCache;

        #endregion

        #region Public Properties

        // TypeDesc<T>-based Converter Cache
        public TypeDescBasedTConverterCache TypeDescBasedTConverterCache => _converterCache.Value;

        internal DelegateCache<DoSetDelegate> DoSetDelegateCache => _doSetCache.Value;

        internal DelegateCache<CallPcObjEventSubscriberDelegate> CallPcObjEventSubsCache => _callPcObjEventSubsCache.Value;

        internal DelegateCache<CallPcGenEventSubscriberDelegate> CallPcGenEventSubsCache => _callPcGenEventSubsCache.Value;

        internal DelegateCache<CallPcTypedEventSubscriberDelegate> CallPcTypedEventSubsCache => _callPcTypeEventSubsCache.Value;

        internal DelegateCache<CallPcStandardEventSubscriberDelegate> CallPcStEventSubsCache => _callPcStEventSubsCache.Value;

        internal DelegateCache<CallPChangingEventSubscriberDelegate> CallPChangingEventSubsCache => _callPChangingEventSubsCache.Value;
        
        

        internal DelegateCache<CreatePropFromStringDelegate> CreatePropFromStringCache => _createPropFromStringCache.Value;

        internal DelegateCache<CreatePropWithNoValueDelegate> CreatePropWithNoValCache => _createPropWithNoValCache.Value;

        internal DelegateCache<CreatePropFromObjectDelegate> CreatePropFromObjectCache => _createPropFromObjectCache.Value;


        #endregion

        #region The Static Constructor

        public DelegateCacheProvider()
        {
            // TypeDesc<T>-based Converter Cache
            _converterCache = new Lazy<TypeDescBasedTConverterCache>(() => new TypeDescBasedTConverterCache(), LazyThreadSafetyMode.PublicationOnly);

            // DoSet 
            MethodInfo doSetMethodInfo = typeof(PropBag).GetMethod("DoSetBridge", BindingFlags.Instance | BindingFlags.NonPublic);
            _doSetCache =
                new Lazy<DelegateCache<DoSetDelegate>>
                (
                    () => new DelegateCache<DoSetDelegate>(doSetMethodInfo), LazyThreadSafetyMode.PublicationOnly
                );


            #region Raise Event Delegates

            // PcObject
            MethodInfo callPcObjEventSubscriber_mi = typeof(PropBag).GetMethod("CallPcObjectEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);

            _callPcObjEventSubsCache =
                new Lazy<DelegateCache<CallPcObjEventSubscriberDelegate>>
                (
                    () => new DelegateCache<CallPcObjEventSubscriberDelegate>(callPcObjEventSubscriber_mi), LazyThreadSafetyMode.PublicationOnly
                );

            // PcGen
            MethodInfo callPcGenEventSubscriber_mi = typeof(PropBag).GetMethod("CallPcGenEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);

            _callPcGenEventSubsCache =
                new Lazy<DelegateCache<CallPcGenEventSubscriberDelegate>>
                (
                    () => new DelegateCache<CallPcGenEventSubscriberDelegate>(callPcGenEventSubscriber_mi), LazyThreadSafetyMode.PublicationOnly
                );

            // PcTyped
            MethodInfo callPcTypedEventSubscriber_mi = typeof(PropBag).GetMethod("CallPcTypedEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);

            _callPcTypeEventSubsCache =
                new Lazy<DelegateCache<CallPcTypedEventSubscriberDelegate>>
                (
                    () => new DelegateCache<CallPcTypedEventSubscriberDelegate>(callPcTypedEventSubscriber_mi), LazyThreadSafetyMode.PublicationOnly
                );

            // PcStandard
            MethodInfo callPcStEventSubscriber_mi = typeof(PropBag).GetMethod("CallPcStandardEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);

            _callPcStEventSubsCache =
                new Lazy<DelegateCache<CallPcStandardEventSubscriberDelegate>>
                (
                    () => new DelegateCache<CallPcStandardEventSubscriberDelegate>(callPcStEventSubscriber_mi), LazyThreadSafetyMode.PublicationOnly
                );

            // PcChanging
            MethodInfo callPChangingEventSubscriber_mi = typeof(PropBag).GetMethod("CallPChaningEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);

            _callPChangingEventSubsCache =
                new Lazy<DelegateCache<CallPChangingEventSubscriberDelegate>>
                (
                    () => new DelegateCache<CallPChangingEventSubscriberDelegate>(callPChangingEventSubscriber_mi), LazyThreadSafetyMode.PublicationOnly
                );

            #endregion

            #region Prop / CProp / DTProp Creation

            // Create Prop From String
            MethodInfo createPropNoVal_mi = typeof(APFGenericMethodTemplates).GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic);
            _createPropWithNoValCache =
                new Lazy<DelegateCache<CreatePropWithNoValueDelegate>>
                (
                    () => new DelegateCache<CreatePropWithNoValueDelegate>(createPropNoVal_mi), LazyThreadSafetyMode.PublicationOnly
                );

            // Create Prop With No Value
            MethodInfo createPropFromString_mi = typeof(APFGenericMethodTemplates).GetMethod("CreatePropFromString", BindingFlags.Static | BindingFlags.NonPublic);
            _createPropFromStringCache =
                new Lazy<DelegateCache<CreatePropFromStringDelegate>>
                (
                    () => new DelegateCache<CreatePropFromStringDelegate>(createPropFromString_mi), LazyThreadSafetyMode.PublicationOnly
                );

            // TODO: This is not being used.
            // Create Prop From Object
            MethodInfo createPropFromObject_mi = typeof(APFGenericMethodTemplates).GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic);
            _createPropFromObjectCache =
                new Lazy<DelegateCache<CreatePropFromObjectDelegate>>
                (
                    () => new DelegateCache<CreatePropFromObjectDelegate>(createPropFromObject_mi), LazyThreadSafetyMode.PublicationOnly
                );

            #endregion
        }
        #endregion
    }
}
