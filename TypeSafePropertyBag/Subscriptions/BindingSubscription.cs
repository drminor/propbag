using DRM.TypeSafePropertyBag.LocalBinding;
using System;

namespace DRM.TypeSafePropertyBag
{
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    internal class BindingSubscription<T> : AbstractSubscripton<T>, IBindingSubscription<T>, IEquatable<BindingSubscription<T>>, IEquatable<ISubscription>, IDisposable
    {
        #region IBindingSubscription<T> Implementation

        public LocalBinder<T> LocalBinder { get; }

        #endregion

        #region ISubscription<T> Implementation

        new CallPcTypedEventSubscriberDelegate PcTypedHandlerDispatcher => throw new NotSupportedException($"The {nameof(BindingSubscription<T>)} does not support dispatching EventHandler<PCTypeEventArgs<T>.");

        #endregion

        #region ISubscription Implementation

        new public object Target => null;
        new public Delegate HandlerProxy => null;
        new public object LocalBinderAsObject => (object)LocalBinder;

        #endregion

        #region Constructors

        public BindingSubscription(IBindingSubscriptionKey<T> sKey, PSAccessServiceInterface propStoreAccessService)
        {
            OwnerPropId = sKey.OwnerPropId;
            BindingInfo = sKey.BindingInfo;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            //SubscriptionTargetKind = sKey.SubscriptionTargetKind;

            LocalBinder = new LocalBinder<T>(propStoreAccessService, OwnerPropId, sKey.BindingInfo);
        }

        #endregion

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as BindingSubscription<T>);
        }

        public bool Equals(BindingSubscription<T> other)
        {
            return other != null && object.ReferenceEquals(LocalBinder, other.LocalBinder);
        }

        public override int GetHashCode()
        {
            return LocalBinder.GetHashCode();
        }

        public bool Equals(ISubscription other)
        {
            return other != null && object.ReferenceEquals(LocalBinder, other.LocalBinderAsObject);
        }

        public static bool operator ==(BindingSubscription<T> subscription1, BindingSubscription<T> subscription2)
        {
            return object.ReferenceEquals(subscription1.LocalBinder, subscription2.LocalBinder);
        }

        public static bool operator !=(BindingSubscription<T> subscription1, BindingSubscription<T> subscription2)
        {
            return !(subscription1 == subscription2);
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
                    // TODO: dispose managed state (managed objects).
                    LocalBinder.Dispose();
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
