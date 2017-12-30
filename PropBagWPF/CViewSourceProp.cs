using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropIdType = UInt32;
    using PropNameType = String;

    public class CViewSourceProp : PropTypedBase<CollectionViewSource>, ICViewSourceProp<CollectionViewSource>
    {
        #region Private and Protected Members

        private readonly PropNameType _propertyName;
        private readonly IProvideAView _viewProvider;

        #endregion

        #region Constructor

        public CViewSourceProp(PropNameType propertyName, IProvideAView viewProvider)
            : base(typeof(CollectionViewSource), true, true, true, RefEqualityComparer<CollectionViewSource>.Default.Equals, null, PropKindEnum.CollectionViewSource)
        {
            _propertyName = propertyName;
            _viewProvider = viewProvider;
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

        public event EventHandler<ViewRefreshedEventArgs> ViewRefreshed;

        #region Public Properties and Methods

        override public CollectionViewSource TypedValue
        {
            get
            {
                CollectionViewSource cvs = _viewProvider.ViewSource as CollectionViewSource;
                if(cvs.Source is DataSourceProvider dsp)
                {
                    dsp.DataChanged += Dsp_DataChanged;
                }

                _viewProvider.ViewRefreshed += _viewProvider_ViewRefreshed;
                return cvs; // _viewProvider.ViewSource as CollectionViewSource;
            }

            set
            {
                throw new InvalidOperationException("TODO: Fix Me");
            }
        }

        private void _viewProvider_ViewRefreshed(object sender, ViewRefreshedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CVS received as ViewRefreshed Event.");
        }

        private void Dsp_DataChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CVS's DSP received as ViewRefreshed Event.");
        }

        public override object TypedValueAsObject => TypedValue;

        //public DataSourceProvider Source
        //{
        //    get
        //    {
        //        if (TryGetDataSourceProvider(TypedValue.Source, out DataSourceProvider dsp))
        //        {
        //            return dsp;
        //        }
        //        else
        //        {
        //            throw new InvalidOperationException($"The current CollectionViewSource's Source does not derive from {nameof(DataSourceProvider)} class.");
        //        }
        //    }
        //    set
        //    {
        //        if (TypedValue.Source != null)
        //        {
        //            if (TypedValue.Source is DataSourceProvider dsp)
        //            {
        //                dsp.DataChanged -= Source_DataChanged;
        //            }
        //        }

        //        TypedValue.Source = value;
        //        value.DataChanged += Source_DataChanged;
        //    }
        //}

        //private void Source_DataChanged(object sender, EventArgs e)
        //{
        //    OnViewRefreshed(null);
        //    foreach (KeyValuePair<string, CollectionViewSource> kvp in _views)
        //    {
        //        OnViewRefreshed(kvp.Key);
        //    }
        //}

        //private void OnViewRefreshed(string viewName)
        //{
        //    Interlocked.CompareExchange(ref ViewRefreshed, null, null)?.Invoke(this, new ViewRefreshedEventArgs(viewName));
        //}

        //public ListCollectionView View
        //{
        //    get
        //    {
        //        if (TryGetListCollectionView(TypedValue.View, out ListCollectionView lcv))
        //        {
        //            return lcv;
        //        }
        //        else
        //        {
        //            throw new InvalidOperationException("The view provided by this CollectionViewSource does not implement the ListCollectionView interface.");
        //        }
        //    }
        //}

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

        ICollectionView IProvideAView.View => _viewProvider.View;
        public string ViewName => _viewProvider.ViewName;
        public object ViewSource => _viewProvider.ViewSource;

        #endregion

        //#region IProvideADataSourceProvider implementation

        //DataSourceProvider IProvideADataSourceProvider.DataSourceProvider => throw new NotImplementedException();

        //#endregion

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

        //#region View Management

        //IDictionary<string, CollectionViewSource> _views;

        //public ListCollectionView this[string key]
        //{
        //    get
        //    {
        //        if (_views == null)
        //        {
        //            _views = new Dictionary<string, CollectionViewSource>();
        //        }
        //        else
        //        {
        //            if (_views.TryGetValue(key, out CollectionViewSource theView))
        //            {
        //                if (TryGetListCollectionView(theView.View, out ListCollectionView lcv1))
        //                {
        //                    return lcv1;
        //                }
        //                else
        //                {
        //                    throw new InvalidOperationException($"The view provided by the CollectionViewSource for item: {key} does not implement the ListCollectionView interface.");
        //                }
        //            }
        //        }

        //        CollectionViewSource cvs = new CollectionViewSource
        //        {
        //            Source = TypedValue.Source
        //        };

        //        _views.Add(key, cvs);

        //        ICollectionView view = cvs.View;

        //        if (TryGetListCollectionView(view, out ListCollectionView lcv2))
        //        {
        //            return lcv2;
        //        }
        //        else
        //        {
        //            throw new InvalidOperationException($"The view provided by the CollectionViewSource for item: {key} does not implement the ListCollectionView interface.");
        //        }
        //    }

        //}

        //#endregion
    }
}