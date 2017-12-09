﻿using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    public class Subscription<T> : AbstractSubscripton<T>, IDisposable
    {
        #region Constructors

        public Subscription(ISubscriptionKey<T> sKey)
        {
            OwnerPropId = sKey.OwnerPropId;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            //SubscriptionTargetKind = sKey.SubscriptionTargetKind;

            switch (sKey.SubscriptionKind)
            {
                case SubscriptionKind.TypedHandler:
                    {
                        Target = new WeakReference(sKey.TypedHandler.Target);

                        TypedHandler = (EventHandler<PCTypedEventArgs<T>>)Delegate.CreateDelegate(typeof(EventHandler<PCTypedEventArgs<T>>), null, sKey.TypedHandler.Method);

                        MethodInfo mi = sKey.GenHandler.Method;
                        Delegate proxyDelegate = mi.CreateDelegate(); // Uses static extension Method -- TODO: Provide Instance Library.
                        HandlerProxy = proxyDelegate;


                        MethodName = sKey.TypedHandler.Method.Name;

                        break;
                    }

                //case SubscriptionKind.GenHandler:
                //    break;
                //case SubscriptionKind.ObjHandler:
                //    break;
                //case SubscriptionKind.StandardHandler:
                //    break;

                //case SubscriptionKind.TypedAction:
                //    {
                //        throw new NotImplementedException("Comming soon.");
                //    }

                //case SubscriptionKind.ObjectAction:
                //    break;
                //case SubscriptionKind.ActionNoParams:
                //    break;
                //case SubscriptionKind.LocalBinding:
                //    break;

                default:
                    throw new InvalidOperationException($"The SubscriptionKind: {SubscriptionKind} is not recognized or is not supported.");
            }
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
                    TypedHandler = null;
                    TypedDoWhenChanged = null;
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
