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

        private readonly CollectionViewSource _viewSource;

        #endregion

        #region Constructor

        public ViewProvider(string viewName, DataSourceProvider dataSourceProvider)
        {
            ViewName = viewName;
            DataSourceProvider = dataSourceProvider ?? throw new ArgumentNullException($"{nameof(dataSourceProvider)} must have a value.");
            _viewSource = new CollectionViewSource()
            {
                Source = DataSourceProvider
            };

            DataSourceProvider.DataChanged += _dataSourceProvider_DataChanged;
        }

        private void _dataSourceProvider_DataChanged(object sender, EventArgs e)
        {
            string viewName = null;
            OnViewSourceRefreshed(viewName);

            //// Automatically ask the View to requery it's data.
            //_viewSource.View.Refresh();
        }

        #endregion

        #region Event Declaration and Invoker

        public event EventHandler<ViewRefreshedEventArgs> ViewSourceRefreshed;

        private void OnViewSourceRefreshed(string viewName)
        {
            Interlocked.CompareExchange(ref ViewSourceRefreshed, null, null)?.Invoke(this, new ViewRefreshedEventArgs(viewName));
        }

        #endregion

        #region Public Properties and Methods

        public DataSourceProvider DataSourceProvider { get; }

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

        public CollectionViewSource /*IProvideACollectionViewSource.*/CollectionViewSource => _viewSource;

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
