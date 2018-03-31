using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    internal class ParentNCSubscription : IDisposable
    {
        #region Constructors

        //public ParentNCSubscription(ExKeyT ownerPropId, WeakRefKey target, string methodName, Delegate proxy, CallPSParentNodeChangedEventSubDelegate dispatcher)
        //{
        //    OwnerPropId = ownerPropId;
        //    Target = target;
        //    MethodName = methodName;
        //    Proxy = proxy;
        //    Dispatcher = dispatcher;
        //}

        public ParentNCSubscription(ParentNCSubscriptionRequest subRequestKey, ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> callPSParentNodeChangedEventSubsCache)
        {
            OwnerPropId = subRequestKey.OwnerPropId;
            Target_Wrk = subRequestKey.Target_Wrk; // new WeakRefKey(subRequestKey.Target_Wrk);
            MethodName = subRequestKey.Method.Name;

            // TODO: Note: The Proxy Delegate is not being cached (using the DelegateProxyCache.)
            // Create an open delegate from the delegate provided. (An open delegate has a null value for the target.)
            Type delegateType = GetDelegateType(subRequestKey.Method);
            Proxy = MakeTheDelegate(delegateType, subRequestKey.Method);

            Type targetType = subRequestKey.Target_Wrk.GetType();
            Dispatcher = callPSParentNodeChangedEventSubsCache.GetOrAdd(targetType);
        }

        #endregion

        #region Public Properties

        public ExKeyT OwnerPropId { get; } // The Node that raises the event.

        public WeakRefKey Target_Wrk { get; }
        public object Target => Target_Wrk.Target;

        public string MethodName { get; }

        public Delegate Proxy { get; private set; }

        public CallPSParentNodeChangedEventSubDelegate Dispatcher { get; }


        #endregion

        #region Private Methods

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
            System.Diagnostics.Debug.WriteLine($"Creating Delegate: {delegateType} for {method.Name}. (ParentNCSubscription.");
            Delegate result = Delegate.CreateDelegate(delegateType, null, method);
            return result;
        }

        private void CallParentNodeChangedEventSubscriber<TCaller>(object target, object sender, PSNodeParentChangedEventArgs e, Delegate d)
        {
            Action<TCaller, object, PSNodeParentChangedEventArgs> realDel = (Action<TCaller, object, PSNodeParentChangedEventArgs>)d;

            realDel((TCaller)target, sender, e);
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    Target_Wrk.Clear();
                    Proxy = null;
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
