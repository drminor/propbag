﻿using DRM.TypeSafePropertyBag.DelegateCaches;
using System;

namespace DRM.TypeSafePropertyBag.LocalBinding
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public class LocalBinder<T> : IDisposable
    {
        const string BINDER_NAME = "PB_LocalBinder";

        #region Private Properties

        readonly ExKeyT _bindingTarget;
        readonly WeakRefKey<IPropBag> _targetObject;
        readonly PropNameType _propertyName;

        readonly LocalBindingInfo _bindingInfo;

        PropStorageStrategyEnum _targetStorageStrategy;

        readonly LocalWatcher<T> _localWatcher;

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

        public LocalBinder(PSAccessServiceInterface propStoreAccessService, ExKeyT bindingTarget, LocalBindingInfo bindingInfo)
        {
            _bindingTarget = bindingTarget;
            _bindingInfo = bindingInfo;

            // Get the PropStore Node for the IPropBag object hosting the property that is the target of the binding.
            BagNode ourNode = GetPropBagNode(propStoreAccessService);

            // Get a weak reference to the PropBag hosting the target property.
            _targetObject = ourNode.PropBagProxy;

            // Get the name of the target property from the PropId given to us.
            if (_targetObject.TryGetTarget(out IPropBag propBag))
            {
                PropIdType propId = _bindingTarget.Level2Key;

                _propertyName = GetPropertyName(propStoreAccessService, propBag, propId, out PropStorageStrategyEnum storageStrategy);

                if(storageStrategy == PropStorageStrategyEnum.External)
                {
                    throw new InvalidOperationException($"{storageStrategy} is not a supported Prop Storage Strategy when used as a target of a local binding.");
                }

                // We will update the target property depending on how that PropItem stores its value.
                _targetStorageStrategy = storageStrategy;

                // Create a instance of our nested, internal class that reponds to Updates to the property store Nodes.
                IReceivePropStoreNodeUpdates_PropNode<T> propStoreNodeUpdateReceiver = new PropStoreNodeUpdateReceiver(this);

                // Create a new watcher, the bindingInfo specifies the PropItem for which to listen to changes,
                // the propStoreNodeUpdateReceiver will be notfied when changes occur.
                _localWatcher = new LocalWatcher<T>(propStoreAccessService, bindingInfo, propStoreNodeUpdateReceiver);

            }
            else
            {
                // TODO: consider creating a TryCreateLocalBinding to avoid this situation.
                System.Diagnostics.Debug.WriteLine("The target was found to have been Garbage Collected when creating a Local Binding.");
            }
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

        private IConvertValues GetConverter(WeakRefKey<IPropBag> bindingTarget, LocalBindingInfo bInfo, Type sourceType,
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

        private object OurDefaultConverterParameterBuilder(WeakRefKey<IPropBag> bindingTarget, LocalBindingInfo bInfo, Type sourceType,
            string pathElement)
        {
            //return new TwoTypes(sourceType, bindingTarget.PropertyType);
            return null;
        }

        #endregion

        #region Update Target

        private bool UpdateTargetWithStartingValue(WeakRefKey<IPropBag> bindingTarget, PropNode sourcePropNode)
        {
            IProp typedProp = sourcePropNode.PropData_Internal.TypedProp;
            PropStorageStrategyEnum ss = typedProp.PropTemplate.StorageStrategy;

            switch (ss)
            {
                case PropStorageStrategyEnum.Internal:
                    {
                        T newValue = (T)typedProp.TypedValueAsObject;
                        bool result = UpdateTarget(bindingTarget, newValue);
                        return result;
                    }

                case PropStorageStrategyEnum.External:
                    goto case PropStorageStrategyEnum.Internal;

                // This property has no backing store, there is no concept of a starting value.
                case PropStorageStrategyEnum.Virtual:
                    return false;

                default:
                    throw new InvalidOperationException($"{ss} is not a recognized or supported Prop Storage Strategy when used as a source for a local binding.");
            }
        }

        private bool UpdateTarget(WeakRefKey<IPropBag> bindingTarget, T newValue)
        {
            if (bindingTarget.TryGetTarget(out IPropBag propBag))
            {
                bool wasSet;

                switch (_targetStorageStrategy)
                {
                    case PropStorageStrategyEnum.Internal:
                        wasSet = (propBag).SetIt(newValue, this.PropertyName);
                        break;

                    case PropStorageStrategyEnum.External:
                        goto case PropStorageStrategyEnum.Virtual;

                    case PropStorageStrategyEnum.Virtual:
                        T dummy = default(T);
                        wasSet = (propBag).SetIt(newValue, ref dummy, this.PropertyName);
                        break;

                    default:
                        throw new InvalidOperationException($"{_targetStorageStrategy} is not a recognized or supported Prop Storage Strategy when used as a target of a local binding.");
                }

                ReportUpdateStatus(wasSet);
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Target IPropBag has been Garbage Collected on call to Update Target.");
                return false;
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void ReportUpdateStatus(bool wasSet)
        {
            string status = wasSet ? "has been updated" : "already had the new value";
            System.Diagnostics.Debug.WriteLine($"The binding target {status}.");

        }

        #endregion

        #region Private Methods

        private BagNode GetPropBagNode(PSAccessServiceInterface propStoreAccessService)
        {
            CheckForIHaveTheStoreNode(propStoreAccessService);
            return ((IHaveTheStoreNode)propStoreAccessService).PropBagNode;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckForIHaveTheStoreNode(PSAccessServiceInterface propStoreAccessService)
        {
            if (!(propStoreAccessService is IHaveTheStoreNode storeNodeProvider))
            {
                throw new InvalidOperationException($"The {nameof(propStoreAccessService)} does not implement the {nameof(IHaveTheStoreNode)} interface.");
            }
        }

        // TODO: should be able to have IPropStoreAccessServiceInternal provide all of this with a single call.
        private PropNameType GetPropertyName(PSAccessServiceInterface propStoreAccessService, IPropBag propBag, PropIdType propId, out PropStorageStrategyEnum storageStrategy)
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

            if (propStoreAccessService.TryGetValue(propBag, propId, out IPropData genProp))
            {
                storageStrategy = genProp.TypedProp.PropTemplate.StorageStrategy;
            }
            else
            {
                throw new InvalidOperationException("Cannot retrieve the Target's property name from the TargetPropId.");
            }

            return result;
        }

        private bool TryGetPropBag(BagNode objectNode, out IPropBag propBag)
        {
            // Unwrap the weak reference held by the objectNode.
            bool result = objectNode.TryGetPropBag(out propBag);

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

            public void OnPropStoreNodeUpdated(PropNode sourcePropNode, T newValue)
            {
                _localBinder.UpdateTarget(_localBinder._targetObject, newValue);
                //DoUpdate(sourcePropNode);
            }

            public void OnPropStoreNodeUpdated(PropNode sourcePropNode)
            {
                _localBinder.UpdateTargetWithStartingValue(_localBinder._targetObject, sourcePropNode);

                //DoUpdate(sourcePropNode);
            }

            //private void DoUpdate(PropNode sourcePropNode)
            //{
            //    _localBinder.UpdateTargetWithStartingValue(_localBinder._targetObject, sourcePropNode);
            //}
        }

        #endregion
    }
}
