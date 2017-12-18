using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    internal class SubscriptionGen : ISubscription 
    {
        public ExKeyT OwnerPropId { get; set; }
        public Type PropertyType { get; }

        public SubscriptionKind SubscriptionKind { get; }
        public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; set; }
        //public SubscriptionTargetKind SubscriptionTargetKind { get; protected set; }

        public WeakReference Target { get; protected set; }
        public string MethodName { get; }

        public Delegate HandlerProxy { get; }

        public CallPcTypedEventSubscriberDelegate PcTypedHandlerDispatcher { get; }
        public CallPcGenEventSubscriberDelegate PcGenHandlerDispatcher { get; }
        public CallPcObjEventSubscriberDelegate PcObjHandlerDispatcher { get; }
        public CallPcStandardEventSubscriberDelegate PcStandardHandlerDispatcher { get; }
        public CallPChangingEventSubscriberDelegate PChangingHandlerDispatcher { get; }

        //public Action<object, object> GenDoWhenChanged { get; protected set; }
        //public Action Action { get; protected set; }

        // Binding Subscription Members
        public LocalBindingInfo BindingInfo => throw new InvalidOperationException("SubscriptionGen cannot be used for BindingSubscriptions.");
        public object LocalBinderAsObject => throw new InvalidOperationException("SubscriptionGen cannot be used for BindingSubscriptions.");

        public SubscriptionGen(ISubscriptionKeyGen sKey, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            if(sKey.HasBeenUsed)
            {
                throw new InvalidOperationException("The Key has already been used.");
            }

            OwnerPropId = sKey.OwnerPropId;
            PropertyType = sKey.PropertyType;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            //SubscriptionTargetKind = sKey.SubscriptionTargetKind;

            switch (SubscriptionKind)
            {
                case SubscriptionKind.TypedHandler:
                    {
                        Target = new WeakReference(sKey.Target);

                        Delegate proxyDelegate = handlerDispatchDelegateCacheProvider.DelegateProxyCache.GetOrAdd(new MethodSubscriptionKind(sKey.Method, sKey.SubscriptionKind));
                        HandlerProxy = proxyDelegate;

                        Type targetType = sKey.Target.GetType();

                        TypePair tp = new TypePair(targetType, PropertyType);

                        CallPcTypedEventSubscriberDelegate callTheListenerDel =
                            handlerDispatchDelegateCacheProvider.CallPcTypedEventSubsCache.GetOrAdd(tp);

                        PcTypedHandlerDispatcher = callTheListenerDel;

                        MethodName = HandlerProxy.Method.Name;
                        break;
                    }

                case SubscriptionKind.GenHandler:
                    {
                        //Target = new WeakReference(sKey.GenHandler.Target);
                        Target = new WeakReference(sKey.Target);

                        Delegate proxyDelegate = handlerDispatchDelegateCacheProvider.DelegateProxyCache.GetOrAdd(new MethodSubscriptionKind(sKey.Method, sKey.SubscriptionKind));
                        HandlerProxy = proxyDelegate;

                        //Type targetType = sKey.GenHandler.Target.GetType();
                        Type targetType = sKey.Target.GetType();

                        PcGenHandlerDispatcher = handlerDispatchDelegateCacheProvider.CallPcGenEventSubsCache.GetOrAdd(targetType);

                        MethodName = HandlerProxy.Method.Name;
                        break;
                    }
                case SubscriptionKind.ObjHandler:
                    {
                        //Target = new WeakReference(sKey.ObjHandler.Target);
                        Target = new WeakReference(sKey.Target);

                        Delegate proxyDelegate = handlerDispatchDelegateCacheProvider.DelegateProxyCache.GetOrAdd(new MethodSubscriptionKind(sKey.Method, sKey.SubscriptionKind));
                        HandlerProxy = proxyDelegate;

                        //Type targetType = sKey.ObjHandler.Target.GetType();
                        Type targetType = sKey.Target.GetType();

                        PcObjHandlerDispatcher = handlerDispatchDelegateCacheProvider.CallPcObjEventSubsCache.GetOrAdd(targetType);

                        MethodName = HandlerProxy.Method.Name;
                        break;
                    }
                case SubscriptionKind.StandardHandler:
                    {
                        //Target = new WeakReference(sKey.StandardHandler.Target);
                        Target = new WeakReference(sKey.Target);


                        Delegate proxyDelegate = handlerDispatchDelegateCacheProvider.DelegateProxyCache.GetOrAdd(new MethodSubscriptionKind(sKey.Method, sKey.SubscriptionKind));
                        HandlerProxy = proxyDelegate;

                        //Type targetType = sKey.StandardHandler.Target.GetType();
                        Type targetType = sKey.Target.GetType();

                        PcStandardHandlerDispatcher = handlerDispatchDelegateCacheProvider.CallPcStEventSubsCache.GetOrAdd(targetType);

                        MethodName = HandlerProxy.Method.Name;
                        break;
                    }

                case SubscriptionKind.ChangingHandler:
                    {
                        //Target = new WeakReference(sKey.ChangingHandler.Target);
                        Target = new WeakReference(sKey.Target);


                        Delegate proxyDelegate = handlerDispatchDelegateCacheProvider.DelegateProxyCache.GetOrAdd(new MethodSubscriptionKind(sKey.Method, sKey.SubscriptionKind));
                        HandlerProxy = proxyDelegate;

                        //Type targetType = sKey.ChangingHandler.Target.GetType();
                        Type targetType = sKey.Target.GetType();

                        PChangingHandlerDispatcher = handlerDispatchDelegateCacheProvider.CallPChangingEventSubsCache.GetOrAdd(targetType);

                        MethodName = HandlerProxy.Method.Name;
                        break;
                    }

                //case SubscriptionKind.TypedAction:
                //    break;
                //case SubscriptionKind.ObjectAction:
                //    break;
                //case SubscriptionKind.ActionNoParams:
                //    break;
                //case SubscriptionKind.LocalBinding:
                //    break;
                default:
                    throw new InvalidOperationException($"The SubscriptionKind: {SubscriptionKind} is not recognized or is not supported.");
            }

            //GenDoWhenChanged = sKey.GenDoWhenChanged;
            //Action = sKey.Action;
        }
    }
}
