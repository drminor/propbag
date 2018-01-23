using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    internal class ObservableSource<T> : INotifyPCTyped<T>, IDisposable
    {
        #region Private Members

        private IDisposable ParentChangedSource { get; }
        private IDisposable PropChangeGenUnsubscriber { get; }
        private IDisposable PropChangedTypedUnsubscriber { get; }

        public WeakReference<IPropBag> LastEventSender { get; private set; }



        //private 

        #endregion

        #region Events

        public event EventHandler<PcTypedEventArgs<T>> PropertyChangedWithTVals;
        public event EventHandler<PcGenEventArgs> PropertyChangedWithVals;
        public event EventHandler ParentHasChanged;

        #endregion

        #region Constructors and their handlers

        #region For PropertyChangedWithTVals

        public ObservableSource(IPropBag propBag, ExKeyT compKey,
            string pathElement, string binderName)
        {
            CompKey = compKey;
            PathElement = pathElement;
            BinderName = binderName;

            SourceKind = SourceKindEnum.TerminalNode;
            IDisposable disable = propBag.SubscribeToPropChanged<T>(PropertyChangedWithTVals_Handler, pathElement);
            PropChangedTypedUnsubscriber = disable;
        }

        private void PropertyChangedWithTVals_Handler(object sender, PcTypedEventArgs<T> e)
        {
            // TODO: Include the original sender in the event data (and create a new event args class for this.)
            LastEventSender = sender as WeakReference<IPropBag>;

            OnPropertyChangedWithTVals(e);
        }

        #endregion

        #region For PropertyChangedWithGenVals

        public ObservableSource(IPropBag propBag, ExKeyT compKey,
            string pathElement, SourceKindEnum sourceKind, string binderName)
        {
            CompKey = compKey;
            PathElement = pathElement;
            BinderName = binderName;

            SourceKind = sourceKind;
            IDisposable disable = propBag.SubscribeToPropChanged(PropertyChangedWithGenVals_Handler, PathElement, typeof(T));
            PropChangeGenUnsubscriber = disable;
        }

        private void PropertyChangedWithGenVals_Handler(object sender, PcGenEventArgs e)
        {
            OnPropertyChangedWithGenVals(e);
        }

        #endregion

        #region For PropStore

        public ObservableSource(INotifyParentNodeChanged notifyParentChangedSource, ExKeyT compKey, 
            string pathElement, SourceKindEnum sourceKind, string binderName)
        {
            CompKey = compKey;
            PathElement = pathElement;
            BinderName = binderName;

            SourceKind = sourceKind;

            if(sourceKind == SourceKindEnum.AbsRoot)
            {
                // TODO: Subscribe to RootNodeChanged instead.
                ParentChangedSource = notifyParentChangedSource.SubscribeToParentNodeHasChanged(ParentNodeHasChanged_Handler);
            }
            else
            {
                ParentChangedSource = notifyParentChangedSource.SubscribeToParentNodeHasChanged(ParentNodeHasChanged_Handler);
            }
        }

        private void ParentNodeHasChanged_Handler(object sender, PSNodeParentChangedEventArgs e)
        {
            OnParentHasChanged();
        }

        #endregion

        #endregion Constructors and their handlers

        #region Public Properties and Methods

        public string BinderName { get; private set; }
        public PathConnectorTypeEnum PathConnector => PathConnectorTypeEnum.Dot;

        public ExKeyT CompKey { get; }
        public string PathElement { get; set; }
        public SourceKindEnum SourceKind { get; }

        public bool TryGetStoreAccessor(out WeakReference<PSAccessServiceInterface> propStoreAccessService_wr)
        {
            if (SourceKind == SourceKindEnum.TerminalNode)
            {
                if (PropChangedTypedUnsubscriber is Unsubscriber unsubscriber)
                {
                    propStoreAccessService_wr = unsubscriber._propStoreAccessService_Wr;
                    return true;
                }
            }
            propStoreAccessService_wr = null;
            return false;
        }

        #endregion

        #region Private Methods

        private void RemoveSubscriptions()
        {
            switch (this.SourceKind)
            {
                case SourceKindEnum.AbsRoot:
                    {
                        goto case SourceKindEnum.Up;
                    }
                case SourceKindEnum.Up:
                    {
                        ParentChangedSource.Dispose(); // .ParentNodeHasChanged -= ParentNodeHasChanged_Handler;
                        break;
                    }
                case SourceKindEnum.Down:
                    {
                        PropChangeGenUnsubscriber.Dispose(); // .UnSubscribeToPropChanged(PropertyChangedWithGenVals_Handler, PathElement, typeof(T));
                        break;
                    }
                case SourceKindEnum.TerminalNode:
                    {
                        PropChangedTypedUnsubscriber.Dispose(); // .UnSubscribeToPropChanged<T>(PropertyChangedWithTVals_Handler, PathElement);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"{this.SourceKind} is not recognized or is not supported.");
                    }
            }
        }

        #endregion

        #region Raise Event Helpers

        private void OnPropertyChangedWithTVals(PcTypedEventArgs<T> eArgs)
        {
            Interlocked.CompareExchange(ref PropertyChangedWithTVals, null, null)?.Invoke(this, eArgs);
        }

        private void OnPropertyChangedWithGenVals(PcGenEventArgs eArgs)
        {
            Interlocked.CompareExchange(ref PropertyChangedWithVals, null, null)?.Invoke(this, eArgs);
        }

        private void OnParentHasChanged()
        {
            Interlocked.CompareExchange(ref ParentHasChanged, null, null)?.Invoke(this, EventArgs.Empty);
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
                    ParentHasChanged = null;
                    PropertyChangedWithVals = null;
                    PropertyChangedWithTVals = null;
                    RemoveSubscriptions();
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
