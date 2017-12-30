using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    public class ViewProvider : IProvideAView, IProvideACollectionViewSource 
    {
        #region Private Members

        private readonly DataSourceProvider _dataSourceProvider;
        private readonly CollectionViewSource _viewSource;

        #endregion

        #region Constructor

        public ViewProvider(string viewName, DataSourceProvider dataSourceProvider)
        {
            ViewName = viewName;
            _dataSourceProvider = dataSourceProvider ?? throw new ArgumentNullException($"{nameof(dataSourceProvider)} must have a value.");
            _viewSource = new CollectionViewSource()
            {
                Source = _dataSourceProvider
            };
            _dataSourceProvider.DataChanged += _dataSourceProvider_DataChanged;
        }

        private void _dataSourceProvider_DataChanged(object sender, EventArgs e)
        {
            object i;
            var j = _viewSource.Source;

            if(j is DataSourceProvider dsp)
            {
                i = dsp.Data;
            }
            else
            {
                i = j;
            }


            string viewName = null;
            OnViewRefreshed(viewName);
            _viewSource.View.Refresh();
        }

        #endregion

        public event EventHandler<ViewRefreshedEventArgs> ViewRefreshed;

        #region Public Properties and Methods

        //public DataSourceProvider _dataSourceProvider
        //{
        //    get
        //    {
        //        if (TryGetDataSourceProvider(_viewSource.Source, out DataSourceProvider dsp))
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
        //        if (_viewSource.Source != null)
        //        {
        //            if (_viewSource.Source is DataSourceProvider dsp)
        //            {
        //                dsp.DataChanged -= Source_DataChanged;
        //            }
        //        }

        //        _viewSource.Source = value;
        //        value.DataChanged += Source_DataChanged;
        //    }
        //}

        private void OnViewRefreshed(string viewName)
        {
            Interlocked.CompareExchange(ref ViewRefreshed, null, null)?.Invoke(this, new ViewRefreshedEventArgs(viewName));
        }

        public ListCollectionView View
        {
            get
            {
                if (TryGetListCollectionView(_viewSource.View, out ListCollectionView lcv))
                {
                    return lcv;
                }
                else
                {
                    throw new InvalidOperationException("The view provided by this CollectionViewSource does not implement the ListCollectionView interface.");
                }
            }
        }

        #endregion

        #region IProvideAView implementation

        ICollectionView IProvideAView.View => View;
        public string ViewName { get; }
        public object ViewSource => _viewSource;

        #endregion

        #region IProvideACollectionViewSource Implementation

        CollectionViewSource IProvideACollectionViewSource.CollectionViewSource => _viewSource;

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
                System.Diagnostics.Debug.Assert(ReferenceEquals(lcv, icv),"lcv is not equal to icv");
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
