using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropNameType = String;

    public class CViewProp : PropTypedBase<ListCollectionView>, ICViewProp<ListCollectionView>, IUseAViewProvider
    {
        #region Private and Protected Members

        private IProvideAView _viewProvider;

        #endregion

        #region Constructor

        public CViewProp(PropNameType propertyName, IProvideAView viewProvider, IPropTemplate<ListCollectionView> template)
            : base(propertyName, typeIsSolid: true, template: template)
        {
            _viewProvider = viewProvider;

            if (_viewProvider != null)
            {
                // TODO: Make this a weak event subscription.
                _viewProvider.ViewSourceRefreshed += OurViewProviderGotRefreshed;
            }
        }

        public IProvideAView ViewProvider
        {
            get => _viewProvider;
            set
            {
                if(!ReferenceEquals(_viewProvider, value))
                {
                    string viewName = value.ViewName;
                    if (_viewProvider != null)
                    {
                        if(_viewProvider.ViewName != viewName)
                        {
                            throw new InvalidOperationException($"The view name: {value.ViewName}" +
                                $" from the new IProvideAView does not match the view name:" +
                                $" {_viewProvider.ViewName} from the previous IProvideAView," +
                                $" when setting the ViewProvider property on CViewProp with property name: {PropertyName}.");
                        }

                        _viewProvider.ViewSourceRefreshed -= OurViewProviderGotRefreshed;
                    }

                    // save a reference to the old value.
                    IProvideAView oldViewProvider = _viewProvider;

                    // update the view provider so that future requests will use the new data source.
                    _viewProvider = value;

                    // TODO: Build a unit test that proves the WPF binder is cooperating with this update strategy.
                    // Signal to the WPF binder that the view needs to be refreshed.
                    if (oldViewProvider != null)
                    {
                        oldViewProvider.DataSourceProvider.Refresh();
                    }

                    if (_viewProvider != null)
                    {
                        _viewProvider.ViewSourceRefreshed += OurViewProviderGotRefreshed;
                    }

                    RaiseViewSourceRefreshed(new ViewRefreshedEventArgs(viewName));
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OurViewProviderGotRefreshed(object sender, ViewRefreshedEventArgs e)
        {
            // There may be agents that want to know that the View's data source has been updated.
            RaiseViewSourceRefreshed(e);

            // There may be agents that want to know when our value has changed.
            base.OnValueChanged();
        }

        #endregion

        #region Event Delcarations and Invokers

        public event EventHandler<ViewRefreshedEventArgs> ViewSourceRefreshed;

        private void RaiseViewSourceRefreshed(ViewRefreshedEventArgs e)
        {
            Interlocked.CompareExchange(ref ViewSourceRefreshed, null, null)?.Invoke(this, e);
        }

        #endregion

        #region IProp<ListCollectionView> implementation

        override public ListCollectionView TypedValue
        {
            get
            {
                if (_viewProvider != null)
                {
                    ListCollectionView lcv = (ListCollectionView)_viewProvider.View;
                    System.Diagnostics.Debug.Assert(ReferenceEquals(lcv, _viewProvider.View), "The cast is not the same object as the cast source.");
                    return lcv;
                }
                else
                {
                    IList<object> emptyList = new List<object>();
                    ListCollectionView result = new ListCollectionView(emptyList as IList);
                    return result;
                }
            }

            set
            {
                throw new InvalidOperationException("TODO: Fix Me");
            }
        }

        public override object Clone()
        {
            //throw new NotSupportedException($"{nameof(CViewProp)} Prop Items do not implement the Clone method.");

            object result = new CViewProp(this.PropertyName, null, this._template);
            return result;
        }

        public override void CleanUpTyped()
        {
            if (TypedValue is IDisposable disable)
            {
                disable.Dispose();
            }

            if (_viewProvider != null)
            {
                _viewProvider.ViewSourceRefreshed -= OurViewProviderGotRefreshed;
            }
        }

        #endregion

        #region IProvideAView implementation

        public ICollectionView /*IProvideAView.*/View => TypedValue;
        public string ViewName => _viewProvider?.ViewName;
        public object ViewSource => _viewProvider?.ViewSource;
        public DataSourceProvider DataSourceProvider => _viewProvider?.DataSourceProvider;

        #endregion
    }
}
         