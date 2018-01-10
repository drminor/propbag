using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class ParentNCSubscription
    {
        public ExKeyT OwnerPropId { get; } // The Node that raises the event.

        public WeakRefKey Target { get; }
        public string MethodName { get; }

        public Delegate Proxy { get; }

        public CallPSParentNodeChangedEventSubDelegate Dispatcher { get; }

        //public ParentNCSubscription(ExKeyT ownerPropId, WeakRefKey target, string methodName, Delegate proxy, CallPSParentNodeChangedEventSubDelegate dispatcher)
        //{
        //    OwnerPropId = ownerPropId;
        //    Target = target;
        //    MethodName = methodName;
        //    Proxy = proxy;
        //    Dispatcher = dispatcher;
        //}

        public ParentNCSubscription(ParentNCSubscriptionRequest request, ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> callPSParentNodeChangedEventSubsCache)
        {
            Target = new WeakRefKey(request.Target);
            MethodName = request.Method.Name;

            // TODO: Note: The Proxy Delegate is not being cached (using the DelegateProxyCache.)
            // Create an open delegate from the delegate provided. (An open delegate has a null value for the target.)
            Type delegateType = GetDelegateType(request.Method);
            Proxy = MakeTheDelegate(delegateType, request.Method);

            Type targetType = request.Target.GetType();
            Dispatcher = callPSParentNodeChangedEventSubsCache.GetOrAdd(targetType);
        }

        private Type GetDelegateType(MethodInfo method)
        {
            Type result = Expression.GetDelegateType
                (
                new Type[] { method.ReflectedType }
                .Concat(method.GetParameters().Select(p => p.ParameterType)
                .Concat(new Type[] { method.ReturnType })
                )
                .ToArray());
            return result;
        }

        internal Delegate MakeTheDelegate(Type delegateType, MethodInfo method)
        {
            Delegate result = Delegate.CreateDelegate(delegateType, null, method);
            return result;
        }


        private void CallParentNodeChangedEventSubscriber<TCaller>(object target, object sender, PSNodeParentChangedEventArgs e, Delegate d)
        {
            Action<TCaller, object, PSNodeParentChangedEventArgs> realDel = (Action<TCaller, object, PSNodeParentChangedEventArgs>)d;

            realDel((TCaller)target, sender, e);
        }
    }
}
