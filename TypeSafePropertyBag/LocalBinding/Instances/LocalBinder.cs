using System;

namespace DRM.TypeSafePropertyBag.LocalBinding
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceInternalType = IPropStoreAccessServiceInternal<UInt32, String>;

    public class LocalBinder<T> : IDisposable
    {
        const string BINDER_NAME = "PB_LocalBinder";

        #region Private Properties

        //readonly WeakReference<PSAccessServiceType> _propStoreAccessService_wr;

        //readonly StoreNodeBag _ourNode;

        readonly ExKeyT _bindingTarget;
        readonly WeakReference<IPropBagInternal> _targetObject;
        readonly PropNameType _propertyName;

        readonly LocalBindingInfo _bindingInfo;

        PropStorageStrategyEnum _targetHasStore;

        readonly LocalWatcher<T> _localWatcher;

        //ObservableSource<T> _rootListener;
        //readonly OSCollection<T> _pathListeners;
        //string[] _pathElements;

        //bool _isPathAbsolute;
        //bool _isComplete;
        //int _firstNamedStepIndex;

        #endregion

        #region Public Properties

        public ExKeyT BindingTarget => _bindingTarget;

        public LocalBindingInfo BindingInfo => _bindingInfo;
        public bool PathIsAbsolute => _localWatcher.PathIsAbsolute;
        public bool Complete => _localWatcher.Complete;

        public PropIdType PropId => _bindingTarget.Level2Key;
        public PropNameType PropertyName => _propertyName;

        #endregion

        #region Converter Properties

        //Lazy<IValueConverter> _defaultConverter;
        //public virtual Lazy<IValueConverter> DefaultConverter
        //{
        //    get
        //    {
        //        if(_defaultConverter == null)
        //        {
        //            return new Lazy<IValueConverter>(() => new PropValueConverter());
        //        }
        //        return _defaultConverter;
        //    }
        //    set
        //    {
        //        _defaultConverter = value;
        //    }
        //}

        //Func<BindingTarget, MyBindingInfo, Type, string, object> _defConvParamBuilder;
        //public virtual Func<BindingTarget, MyBindingInfo, Type, string, object> DefaultConverterParameterBuilder
        //{
        //    get
        //    {
        //        if (_defConvParamBuilder == null)
        //        {
        //            return OurDefaultConverterParameterBuilder;
        //        }
        //        return _defConvParamBuilder;
        //    }
        //    set
        //    {
        //        _defConvParamBuilder = value;
        //    }
        //}
        #endregion

        #region Constructor

        public LocalBinder(PSAccessServiceType propStoreAccessService, ExKeyT bindingTarget, LocalBindingInfo bindingInfo)
        {
            //_propStoreAccessService_wr = new WeakReference<PSAccessServiceType>(propStoreAccessService);

            _bindingTarget = bindingTarget;
            _bindingInfo = bindingInfo;

            // Get the PropStore Node for the IPropBag object hosting the property that is the target of the binding.
            StoreNodeBag ourNode = GetPropBagNode(propStoreAccessService);

            // Get a weak reference to the PropBag hosting the target property.
            _targetObject = ourNode.PropBagProxy;

            // Get the name of the target property from the PropId given to us.
            if (_targetObject.TryGetTarget(out IPropBagInternal propBag))
            {
                PropIdType propId = _bindingTarget.Level2Key;
                _propertyName = GetPropertyName(propStoreAccessService, propBag, propId, out PropStorageStrategyEnum storageStrategy);
                
                // We will update the target property depending on how that PropItem stores its value.
                _targetHasStore = storageStrategy;
            }

            // Create a instance of our nested, internal class that reponds to Updates to the property store Nodes.
            IReceivePropStoreNodeUpdates_PropNode<T> propStoreNodeUpdateReceiver = new PropStoreNodeUpdateReceiver(this);

            // Create a new watcher, the bindingInfo specifies the PropItem for which to listen to changes,
            // the propStoreNodeUpdateReceiver will be notfied when changes occur.
            _localWatcher = new LocalWatcher<T>(propStoreAccessService, bindingInfo, propStoreNodeUpdateReceiver);
        }

        #endregion

        #region Value Converter Support

        private void SetDefaultConverter()
        {
            //if (DefaultConverter == null)
            //{
            //    if (DefaultConverterParameterBuilder != null)
            //    {
            //        throw new InvalidOperationException("The DefaultParameterBuilder has been given a value, but the DefaultConverter is unassigned.");
            //    }
            //    DefaultConverter = new Lazy<IValueConverter>(() => new PropValueConverter());
            //    DefaultConverterParameterBuilder = OurDefaultConverterParameterBuilder;
            //}
        }

        private IConvertValues GetConverter(WeakReference<IPropBagInternal> bindingTarget, LocalBindingInfo bInfo, Type sourceType,
            string pathElement, bool isPropBagBased, out object converterParameter)
        {

            //if (isPropBagBased)
            //{
            //    if (bInfo.Converter != null)
            //    {
            //        // Use the one specified in the MarkUp
            //        converterParameter = bInfo.ConverterParameterBuilder(bindingTarget, bInfo, sourceType, pathElement);
            //        return bInfo.Converter;
            //    }
            //    else
            //    {
            //        // Use the default for ItemsSource
            //        if(bindingTarget.DependencyProperty.Name == "SelectedItem")
            //        {
            //            // If this is being used to supply a SelectedItem property.
            //            converterParameter = null;
            //            return new SelectedItemConverter();
            //        }
            //        else
            //        {
            //            // Use the default converter provided by the caller or if no default provide our standard PropValueConverter.
            //            converterParameter = DefaultConverterParameterBuilder(bindingTarget, bInfo, sourceType, pathElement);
            //            return DefaultConverter.Value;
            //        }
            //    }
            //}
            //else
            //{
            //    // If no converter specified and the target is SelectedItem,
            //    // use the SelectedItemConverter.
            //    if (bInfo.Converter == null && bindingTarget.DependencyProperty?.Name == "SelectedItem")
            //    {
            //        converterParameter = null;
            //        return new SelectedItemConverter();
            //    }
            //    else
            //    {
            //        // TODO: Check this. I beleive that the Microsoft Binder supplies a default converter.

            //        // If no converter specified in the markup, no converter will be used.
            //        converterParameter = bInfo.ConverterParameterBuilder(bindingTarget, bInfo, sourceType, pathElement);
            //        return bInfo.Converter;
            //    }
            //}
            converterParameter = null;
            return null;
        }

        private object OurDefaultConverterParameterBuilder(WeakReference<IPropBagInternal> bindingTarget, LocalBindingInfo bInfo, Type sourceType,
            string pathElement)
        {
            //return new TwoTypes(sourceType, bindingTarget.PropertyType);
            return null;
        }

        #endregion

        #region Update Target

        private bool UpdateTargetWithStartingValue(WeakReference<IPropBagInternal> bindingTarget, StoreNodeProp sourcePropNode)
        {
            IProp typedProp = sourcePropNode.PropData_Internal.TypedProp;

            if (typedProp.StorageStrategy == PropStorageStrategyEnum.Internal)
            {
                T newValue = (T)typedProp.TypedValueAsObject;
                bool result = UpdateTarget(bindingTarget, newValue);
                return result;
            }
            else
            {
                // This property has no backing store, there is no concept of a starting value. (Propably used to send messages.)
                return false;
            }
        }

        private bool UpdateTarget(WeakReference<IPropBagInternal> bindingTarget, T newValue)
        {
            if (bindingTarget.TryGetTarget(out IPropBagInternal propBag))
            {
                string status;
                if(_targetHasStore == PropStorageStrategyEnum.Internal)
                {
                    bool wasSet = ((IPropBag)propBag).SetIt(newValue, this.PropertyName);
                    status = wasSet ? "has been updated" : "already had the new value";
                }
                else
                {
                    T dummy = default(T);
                    bool wasSet = ((IPropBag)propBag).SetIt(newValue, ref dummy, this.PropertyName);
                    status = wasSet ? "has been updated" : "already had the new value";
                }

                System.Diagnostics.Debug.WriteLine($"The binding target {status}.");
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Target IPropBag was found to be 'not with us' on call to Update Target.");
                return false;
            }
        }

        #endregion

        #region Private Methods

        private StoreNodeBag GetPropBagNode(PSAccessServiceType propStoreAccessService)
        {
            if (propStoreAccessService is IHaveTheStoreNode storeNodeProvider)
            {
                StoreNodeBag propStoreNode = storeNodeProvider.PropStoreNode;
                return propStoreNode;
            }
            else
            {
                throw new InvalidOperationException($"The {nameof(propStoreAccessService)} does not implement the {nameof(IHaveTheStoreNode)} interface.");
            }
        }

        // TODO: should be able to have IPropStoreAccessServiceInternal provide all of this with a single call.
        private PropNameType GetPropertyName(PSAccessServiceType propStoreAccessService, IPropBagInternal propBag, PropIdType propId, out PropStorageStrategyEnum storageStrategy)
        {
            PropNameType result;

            if (propStoreAccessService.TryGetPropName(propId, out PropNameType propertyName))
            {
                result = propertyName;
            }
            else
            {
                throw new InvalidOperationException("Cannot retrieve the Target's property name from the TargetPropId.");
            }

            if (propStoreAccessService.TryGetValue((IPropBag)propBag, propId, out IPropData genProp))
            {
                storageStrategy = genProp.TypedProp.StorageStrategy;
            }
            else
            {
                throw new InvalidOperationException("Cannot retrieve the Target's property name from the TargetPropId.");
            }

            return result;
        }

        private bool TryGetPropBag(StoreNodeBag objectNode, out IPropBagInternal propBag)
        {
            // Unwrap the weak reference held by the objectNode.
            bool result = objectNode.TryGetPropBag(out propBag);

            return result;
        }

        private bool TryGetChildProp(StoreNodeBag objectNode, IPropBagInternal propBag, string propertyName, out StoreNodeProp child)
        {
            PropIdType propId = ((PSAccessServiceInternalType)propBag.ItsStoreAccessor).Level2KeyManager.FromRaw(propertyName);
            bool result = objectNode.TryGetChild(propId, out child);
            return result;
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

        internal class PropStoreNodeUpdateReceiver : IReceivePropStoreNodeUpdates_PropNode<T>
        {
            private readonly LocalBinder<T> _localBinder;

            #region Constructor

            public PropStoreNodeUpdateReceiver(LocalBinder<T> localBinder)
            {
                _localBinder = localBinder;
            }

            #endregion

            //// From the PropBag that holds the source PropItem
            //public void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent_wr, T oldValue)
            //{
            //    DoUpdate(propItemParent_wr);
            //}

            //public void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent_wr)
            //{
            //    DoUpdate(propItemParent_wr);
            //}

            public void OnPropStoreNodeUpdated(StoreNodeProp sourcePropNode, T oldValue)
            {
                DoUpdate(sourcePropNode);
            }

            public void OnPropStoreNodeUpdated(StoreNodeProp sourcePropNode)
            {
                DoUpdate(sourcePropNode);
            }

            private void DoUpdate(StoreNodeProp sourcePropNode)
            {
                _localBinder.UpdateTargetWithStartingValue(_localBinder._targetObject, sourcePropNode);
            }

            //private void DoUpdate(WeakReference<IPropBag> propItemParent_wr)
            //{
            //    _localBinder.UpdateTargetWithStartingValue(_localBinder._targetObject, propItemParent_wr);
            //}
        }

        #endregion
    }
}
