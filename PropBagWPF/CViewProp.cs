using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropNameType = String;

    public class CViewProp : PropTypedBase<ListCollectionView>, ICViewProp<ListCollectionView>
    {
        #region Private and Protected Members

        private readonly PropNameType _propertyName;
        private IProvideAView _viewProvider;

        #endregion

        #region Constuctor
        // TODO: The propertName is not used, we can remove it from the constructor.
        public CViewProp(PropNameType propertyName, IProvideAView viewProvider)
            : base(typeof(ListCollectionView), true, PropStorageStrategyEnum.Virtual, true,
                  RefEqualityComparer<ListCollectionView>.Default.Equals, null, PropKindEnum.CollectionView)
        {
            _propertyName = propertyName;
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
                    bool shouldCallReferesh;
                    if (_viewProvider != null)
                    {
                        if(_viewProvider.ViewName != value.ViewName)
                        {
                            throw new InvalidOperationException("Fix Me");
                        }
                        _viewProvider.ViewSourceRefreshed -= OurViewProviderGotRefreshed;
                        shouldCallReferesh = true;
                    }
                    else
                    {
                        shouldCallReferesh = false;
                    }
                    _viewProvider = value;

                    if (_viewProvider != null)
                    {
                        _viewProvider.ViewSourceRefreshed += OurViewProviderGotRefreshed;
                        RaiseViewSourceRefreshed(new ViewRefreshedEventArgs(_viewProvider.ViewName));
                    }
                    else if(shouldCallReferesh)
                    {
                        // New value is null and the previous value was non-null: refresh to reflect that now the list is empty.
                        RaiseViewSourceRefreshed(new ViewRefreshedEventArgs(_viewProvider.ViewName));
                    }
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
                ListCollectionView lcv = (ListCollectionView)_viewProvider.View;
                System.Diagnostics.Debug.Assert(ReferenceEquals(lcv, _viewProvider.View), "The cast is not the same object as the cast source.");
                return lcv;
            }

            set
            {
                throw new InvalidOperationException("TODO: Fix Me");
            }
        }

        public override object TypedValueAsObject => TypedValue;

        public override object Clone() => throw new NotSupportedException($"{nameof(CViewProp)} Prop Items do not implement the Clone method.");

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

        public ICollectionView /*IProvideAView.*/View => _viewProvider.View;
        public string ViewName => _viewProvider.ViewName;
        public object ViewSource => _viewProvider.ViewSource;
        public override DataSourceProvider DataSourceProvider => _viewProvider.DataSourceProvider;

        #endregion
    }
}
         