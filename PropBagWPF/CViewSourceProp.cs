using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropNameType = String;

    public class CViewSourceProp : PropTypedBase<CollectionViewSource>, ICViewSourceProp<CollectionViewSource>
    {
        #region Private and Protected Members

        private readonly IProvideAView _viewProvider;

        #endregion

        #region Constructor

        public CViewSourceProp(PropNameType propertyName, IProvideAView viewProvider)
            : base(propertyName, typeof(CollectionViewSource), true, PropStorageStrategyEnum.Virtual, true, RefEqualityComparer<CollectionViewSource>.Default.Equals, null, PropKindEnum.CollectionViewSource)
        {
            _viewProvider = viewProvider;
        }

        #endregion

        #region Event Handlers

        private void OurViewProviderGotRefreshed(object sender, ViewRefreshedEventArgs e)
        {
            // Let our listeners know.
            RaiseViewRefreshed(e);
        }

        #endregion

        #region Event Declarations and Invokers

        public event EventHandler<ViewRefreshedEventArgs> ViewSourceRefreshed;

        private void RaiseViewRefreshed(ViewRefreshedEventArgs e)
        {
            Interlocked.CompareExchange(ref ViewSourceRefreshed, null, null)?.Invoke(this, e);
        }

        #endregion

        #region IProp<CollectionViewSource> implementation

        override public CollectionViewSource TypedValue
        {
            get
            {
                CollectionViewSource cvs = _viewProvider.ViewSource as CollectionViewSource;
                //if(cvs.Source is DataSourceProvider dsp)
                //{
                //    dsp.DataChanged += Dsp_DataChanged;
                //}

                //_viewProvider.ViewSourceRefreshed += _viewProvider_ViewRefreshed;
                return cvs; 
            }

            set
            {
                throw new InvalidOperationException("TODO: Fix Me");
            }
        }

        //private void _viewProvider_ViewRefreshed(object sender, ViewRefreshedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("CVS received as ViewRefreshed Event.");
        //}

        //private void Dsp_DataChanged(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("CVS's DSP received as ViewRefreshed Event.");
        //}

        public override object TypedValueAsObject => TypedValue;

        public override object Clone() => throw new NotSupportedException($"This Prop Item of type: {typeof(ICViewSourceProp<CollectionViewSource>).Name} does not implement the Clone method.");

        public override void CleanUpTyped()
        {
            if (TypedValue is IDisposable disable)
            {
                disable.Dispose();
            }
        }

        #endregion

        #region IProvideAView implementation

        public ICollectionView /*IProvideAView.*/View => _viewProvider.View;
        public string ViewName => _viewProvider.ViewName;
        public object ViewSource => _viewProvider.ViewSource;
        public DataSourceProvider DataSourceProvider => _viewProvider.DataSourceProvider;

        #endregion

        #region Private Methods

        private bool TryGetDataSourceProvider(object source, out DataSourceProvider dsp)
        {
            if (source == null)
            {
                dsp = null;
                return true;
            }

            if (source is DataSourceProvider dspTest)
            {
                dsp = dspTest;
                return true;
            }
            else
            {
                dsp = null;
                return false;
            }
        }

        private bool TryGetListCollectionView(ICollectionView icv, out ListCollectionView lcv)
        {
            if (icv == null)
            {
                lcv = null;
                return true;
            }

            if (icv is ListCollectionView lcvTest)
            {
                lcv = lcvTest;
                return true;
            }
            else
            {
                lcv = null;
                return false;
            }
        }

        #endregion
    }
}