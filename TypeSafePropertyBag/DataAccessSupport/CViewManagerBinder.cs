using DRM.TypeSafePropertyBag.LocalBinding;
using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    public class CViewManagerBinder<TDal, TSource, TDestination> : IProvideATypedCViewManager<EndEditWrapper<TDestination>, TDestination>, IDisposable
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit
    {
        const string BINDER_NAME = "CViewManager";

        #region Private Properties

        //readonly LocalBindingInfo _bindingInfo;
        //IMapperRequest _mr;                         // The information necessary to create a IPropBagMapper<TSource, TDestination>

        readonly LocalWatcher<TDal> _localWatcher;


        PropBagMapperCreator _propBagMapperCreator; // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
        CViewProviderCreator _viewBuilder;          // Method that can be used to create a IProvideAView from a DataSourceProvider.

        // The Binding Source's PropBag
        WeakReference<IPropBag> _propItemParent_wr { get; set; }

        #endregion

        #region Event Declarations

        public event EventHandler<EventArgs> ViewManagerChanged;

        #endregion

        #region Public Properties

        public IViewManagerProviderKey ViewManagerProviderKey { get; }

        public LocalBindingInfo BindingInfo => ViewManagerProviderKey.BindingInfo;
        public bool PathIsAbsolute => _localWatcher.PathIsAbsolute;
        public bool Complete => _localWatcher.Complete;

        public PropNameType PropertyName => _localWatcher.PropertyName;

        #endregion

        #region Constructor

        public CViewManagerBinder
            (
            PSAccessServiceType propStoreAccessService,
            IViewManagerProviderKey viewManagerProviderKey,
            //LocalBindingInfo bindingInfo,
            //IMapperRequest mr,  // The information necessary to create a IPropBagMapper<TSource, TDestination>
            PropBagMapperCreator propBagMapperCreator,  // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
            CViewProviderCreator viewBuilder            // Method that can be used to create a IProvideAView from a DataSourceProvider.
            )
        {
            //_bindingInfo = bindingInfo;
            //_mr = mr;
            ViewManagerProviderKey = viewManagerProviderKey;
            _propBagMapperCreator = propBagMapperCreator;
            _viewBuilder = viewBuilder;

            // Create a instance of our nested, internal class that reponds to Updates to the property store Nodes.
            IReceivePropStoreNodeUpdates_PropBag<TDal> propStoreNodeUpdateReceiver = new PropStoreNodeUpdateReceiver(this);

            // Create a new watcher, the bindingInfo specifies the PropItem for which to listen to changes,
            // the propStoreNodeUpdateReceiver will be notfied when changes occur.
            _localWatcher = new LocalWatcher<TDal>(propStoreAccessService, ViewManagerProviderKey.BindingInfo, propStoreNodeUpdateReceiver);
        }

        #endregion

        public IManageCViews CViewManager
        {
            get
            {
                if (_propItemParent_wr != null && _propItemParent_wr.TryGetTarget(out IPropBag propBagHost))
                {
                    if (propBagHost is IPropBagInternal propBag_internal)
                    {
                        PSAccessServiceType foreignStoreAccessor = propBag_internal.ItsStoreAccessor;
                        if (foreignStoreAccessor is PSAccessServiceInternalInterface foreignStoreAccesssor_internal)
                        {
                            PropIdType propId = GetPropertyId(foreignStoreAccessor, propBag_internal, PropertyName);
                            IPropDataInternal propData = foreignStoreAccesssor_internal.GetChild(propId)?.PropData_Internal;

                            IManageCViews result = foreignStoreAccessor.GetOrAddViewManager<TDal, TSource, TDestination>
                                (propBagHost, propId, propData as IPropData, ViewManagerProviderKey.MapperRequest, _propBagMapperCreator, _viewBuilder);

                            return result;
                        }
                        else
                        {
                            throw new InvalidOperationException("Fix Me. StoreAcccessor is not a PSAccessServiceInternalInterface.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Fix Me. PropBag not a IPropBagInternal.");
                    }
                }
                else
                {
                    // The PropBagHost has not been set or it "is no longer with us."
                    return null;
                }
            }
        }

        public IManageTypedCViews<EndEditWrapper<TDestination>, TDestination> TypedCViewManager => throw new NotImplementedException();


        #region Private Methods

        private PropIdType GetPropertyId(PSAccessServiceType propStoreAccessService, IPropBagInternal propBag, PropNameType propertyName)
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
            public void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent_wr, WeakReference<IPropBag> oldPropItemParent_wr)
            {
                DoUpdate(propItemParent_wr);
            }

            public void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent_wr)
            {
                DoUpdate(propItemParent_wr);
            }

            private void DoUpdate(WeakReference<IPropBag> propItemParent_wr)
            {
                _localBinder._propItemParent_wr = propItemParent_wr;
                _localBinder.OnViewManagerChanged();
            }
        }

        #endregion
    }
}
