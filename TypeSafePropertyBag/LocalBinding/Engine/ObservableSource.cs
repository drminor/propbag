using System;
using System.Linq;
using System.Threading;

namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;

    public class ObservableSource<T> : INotifyPCTyped<T>, IDisposable
    {
        #region Public events and properties

        //public event EventHandler<DataSourceChangedEventArgs>  DataSourceChanged = null;

        public event EventHandler<PcTypedEventArgs<T>> PropertyChangedWithTVals;
        public event EventHandler<PcGenEventArgs> PropertyChangedWithVals;
        public event EventHandler ParentHasChanged;

        public string BinderName { get; private set; }
        public PathConnectorTypeEnum PathConnector => PathConnectorTypeEnum.Dot;

        public string PathElement { get; set; }

        public ExKeyT CompKey { get; }

        public SourceKindEnum SourceKind { get; }

        public INotifyParentNodeChanged ParentChangedSource { get; }

        public IPropBag PropChangeGenSource { get; }

        public IPropBag PropChangedTypedSource { get; }

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
            PropChangedTypedSource = propBag;
            propBag.SubscribeToPropChanged<T>(PropertyChangedWithTVals_Handler, pathElement);
        }

        private void PropertyChangedWithTVals_Handler(object sender, PcTypedEventArgs<T> e)
        {
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
            PropChangeGenSource = propBag;
            propBag.SubscribeToPropChanged(PropertyChangedWithGenVals_Handler, PathElement, typeof(T));
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
            ParentChangedSource = notifyParentChangedSource;

            if(sourceKind == SourceKindEnum.AbsRoot)
            {
                // TODO: Subscribe to RootNodeChanged instead.
                notifyParentChangedSource.ParentNodeHasChanged += ParentNodeHasChanged_Handler;
            }
            else
            {
                notifyParentChangedSource.ParentNodeHasChanged += ParentNodeHasChanged_Handler;
            }
        }

        private void ParentNodeHasChanged_Handler(object sender, EventArgs e)
        {
            OnParentHasChanged();
        }

        #endregion

        #endregion Constructors and their handlers

        #region Private Methods

        public void RemoveSubscriptions()
        {
            switch (this.SourceKind)
            {
                case SourceKindEnum.AbsRoot:
                    {
                        goto case SourceKindEnum.Up;
                    }
                case SourceKindEnum.Up:
                    {
                        ParentChangedSource.ParentNodeHasChanged -= ParentNodeHasChanged_Handler;
                        break;
                    }
                case SourceKindEnum.Down:
                    {
                        PropChangeGenSource.UnSubscribeToPropChanged(PropertyChangedWithGenVals_Handler, PathElement, typeof(T));
                        break;
                    }
                case SourceKindEnum.TerminalNode:
                    {
                        PropChangedTypedSource.UnSubscribeToPropChanged<T>(PropertyChangedWithTVals_Handler, PathElement);
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

        public void OnPropertyChangedWithTVals(PcTypedEventArgs<T> eArgs)
        {
            Interlocked.CompareExchange(ref PropertyChangedWithTVals, null, null)?.Invoke(this, eArgs);
        }

        private void OnPropertyChangedWithGenVals(PcGenEventArgs eArgs)
        {
            Interlocked.CompareExchange(ref PropertyChangedWithVals, null, null)
                ?.Invoke(this, eArgs);
        }

        private void OnParentHasChanged()
        {
            Interlocked.CompareExchange(ref ParentHasChanged, null, null)
                ?.Invoke(this, EventArgs.Empty);
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
