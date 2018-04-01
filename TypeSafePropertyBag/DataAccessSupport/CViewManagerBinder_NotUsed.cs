using DRM.TypeSafePropertyBag.LocalBinding;
using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public class CViewManagerBinder<TDal, TSource, TDestination> : IProvideACViewManager /*IProvideATypedCViewManager<EndEditWrapper<TDestination>, TDestination>*/, IDisposable
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
    {
        const string BINDER_NAME = "CViewManager";

        #region Private Properties

        readonly LocalWatcher<TDal> _localWatcher;

        PropBagMapperCreator _propBagMapperCreator; // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
        CViewProviderCreator _viewBuilder;          // Method that can be used to create a IProvideAView from a DataSourceProvider.

        // The Binding Source's PropBag
        WeakRefKey<IPropBag> _propItemParent_wr { get; set; }

        #endregion

        #region Event Declarations

        public event EventHandler<EventArgs> ViewManagerChanged;

        #endregion

        #region Constructor

        public CViewManagerBinder
            (
            PSAccessServiceInterface propStoreAccessService,
            IViewManagerProviderKey viewManagerProviderKey,
            PropBagMapperCreator propBagMapperCreator,  // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
            CViewProviderCreator viewBuilder            // Method that can be used to create a IProvideAView from a DataSourceProvider.
            )
        {
            ViewManagerProviderKey = viewManagerProviderKey;
            _propBagMapperCreator = propBagMapperCreator;
            _viewBuilder = viewBuilder;

            // Create a instance of our nested, internal class that reponds to Updates to the property store Nodes.
            IReceivePropStoreNodeUpdates_PropBag<TDal> propStoreNodeUpdateReceiver = new PropStoreNodeUpdateReceiver(this);

            // Create a new watcher, the bindingInfo specifies the PropItem for which to listen to changes,
            // the propStoreNodeUpdateReceiver will be notfied when changes occur.
            _localWatcher = new LocalWatcher<TDal>(propStoreAccessService, ViewManagerProviderKey.BindingInfo, propStoreNodeUpdateReceiver);

            //var x = _propItemParent_wr;

            //var y = _propItemParent_wr.TryGetTarget(out IPropBag target);

            //var z = target;
        }

        #endregion

        #region Public Properties

        public IViewManagerProviderKey ViewManagerProviderKey { get; }

        public LocalBindingInfo BindingInfo => ViewManagerProviderKey.BindingInfo;
        public bool PathIsAbsolute => _localWatcher.PathIsAbsolute;
        public bool Complete => _localWatcher.Complete;

        public PropNameType PropertyName => _localWatcher.PropertyName;

        #endregion

        #region Public Methods

        public IManageCViews CViewManager
        {
            get
            {
                if (_propItemParent_wr != null && _propItemParent_wr.TryGetTarget(out IPropBag propBagHost))
                {
                    IManageCViews result = propBagHost.GetOrAddViewManager_New<TDal, TSource, TDestination>
                        (
                        propertyName: PropertyName,
                        propertyType: null, // We only know that this property implements IDoCRUD<TSource>
                        mapperRequest: ViewManagerProviderKey.MapperRequest
                        );

                    return result;
                }
                else
                {
                    // The PropBagHost has not been set or it "is no longer with us."
                    return null;
                }
            }
        }

        //public IManageTypedCViews<EndEditWrapper<TDestination>, TDestination> TypedCViewManager => throw new NotImplementedException();

        #endregion

        #region Private Methods

        private PropIdType GetPropertyId(PSAccessServiceInterface propStoreAccessService, PropNameType propertyName)
        {
            if (propStoreAccessService.TryGetPropId(propertyName, out PropIdType propId))
            {
                return propId;
            }
            else
            {
                throw new InvalidOperationException("Cannot retrieve the Target's property name from the TargetPropId.");
            }
        }

        #endregion

        #region Raise Event Helpers

        private void OnViewManagerChanged()
        {
            Interlocked.CompareExchange(ref ViewManagerChanged, null, null)?.Invoke(this, EventArgs.Empty);
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
                    if (_localWatcher != null) _localWatcher.Dispose();
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

        #region IReceivePropStoreNodeUpdates Nested Class

        internal class PropStoreNodeUpdateReceiver : IReceivePropStoreNodeUpdates_PropBag<TDal>
        {
            private readonly CViewManagerBinder<TDal, TSource, TDestination> _localBinder;

            #region Constructor

            public PropStoreNodeUpdateReceiver(CViewManagerBinder<TDal, TSource, TDestination> localBinder)
            {
                _localBinder = localBinder;
            }

            #endregion

            // From the PropBag that holds the source PropItem
            public void OnPropStoreNodeUpdated(WeakRefKey<IPropBag> propItemParent_wr, WeakReference<IPropBag> oldPropItemParent_wr)
            {
                DoUpdate(propItemParent_wr);
            }

            public void OnPropStoreNodeUpdated(WeakRefKey<IPropBag> propItemParent_wr)
            {
                DoUpdate(propItemParent_wr);
            }

            private void DoUpdate(WeakRefKey<IPropBag> propItemParent_wr)
            {
                _localBinder._propItemParent_wr = propItemParent_wr;
                _localBinder.OnViewManagerChanged();
            }
        }

        #endregion
    }
}
