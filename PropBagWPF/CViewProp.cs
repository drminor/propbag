using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropIdType = UInt32;
    using PropNameType = String;

    public class CViewProp : PropTypedBase<ListCollectionView>, ICViewProp<ListCollectionView>
    {
        #region Private and Protected Members

        private readonly PropNameType _propertyName;

        private readonly IProvideAView _viewProvider;

        public event EventHandler<ViewRefreshedEventArgs> ViewRefreshed;

        #endregion

        #region Constuctor

        public CViewProp(PropNameType propertyName, IProvideAView viewProvider)
            : base(typeof(ListCollectionView), true, true, true, RefEqualityComparer<ListCollectionView>.Default.Equals, null, PropKindEnum.CollectionView)
        {
            _propertyName = propertyName;
            _viewProvider = viewProvider;

            // TODO: Implement IDisposable or make this a weak event subscription.
            _viewProvider.ViewRefreshed += OurViewProviderGotRefreshed;
        }

        #endregion

        private void OurViewProviderGotRefreshed(object sender, ViewRefreshedEventArgs e)
        {
            // Let our listeners know.
            RaiseViewRefreshed(e);
        }

        private void RaiseViewRefreshed(ViewRefreshedEventArgs e)
        {
            Interlocked.CompareExchange(ref ViewRefreshed, null, null)?.Invoke(this, e);
        }

        override public ListCollectionView TypedValue
        {
            get
            {
                ListCollectionView lcv = _viewProvider.View as ListCollectionView;
                System.Diagnostics.Debug.Assert(ReferenceEquals(lcv, _viewProvider.View), "The cast is not the same object as the cast source.");
                return lcv;
            }

            set
            {
                throw new InvalidOperationException("TODO: Fix Me");
            }
        }

        public override object TypedValueAsObject => TypedValue;


        #region IProvideAView implementation

        ICollectionView IProvideAView.View => _viewProvider.View;
        public string ViewName => _viewProvider.ViewName;
        public object ViewSource => _viewProvider.ViewSource;

        #endregion

        public override object Clone() => throw new NotSupportedException($"{nameof(CViewProp)} Prop Items do not implement the Clone method.");

        public override void CleanUpTyped()
        {
            if (TypedValue is IDisposable disable)
            {
                disable.Dispose();
            }
        }
    }
}
         