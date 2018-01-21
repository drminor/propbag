using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    public class ViewProvider : IProvideAView, IProvideACollectionViewSource, INotifyItemEndEdit, INotifyCollectionChanged
    {
        #region Private Members

        private readonly CollectionViewSource _viewSource;


        #endregion

        #region Constructor

        public ViewProvider(string viewName, DataSourceProvider dataSourceProvider)
        {
            ViewName = viewName;
            DataSourceProvider = dataSourceProvider ?? throw new ArgumentNullException($"{nameof(dataSourceProvider)} must have a value.");

            _listHasBeenRefreshed = true;
            _view = null;
            _viewSource = new CollectionViewSource()
            {
                Source = DataSourceProvider
            };

            DataSourceProvider.DataChanged += _dataSourceProvider_DataChanged;

            if(dataSourceProvider is INotifyItemEndEdit iniee)
            {
                iniee.ItemEndEdit += Iniee_ItemEndEdit;
            }
        }

        private void Iniee_ItemEndEdit(object sender, EventArgs e)
        {
            if(sender is IEditableObject ieo)
            {
                OnItemEndEdit(ieo, e);
            }
            else
            {
                throw new InvalidOperationException("The DataSourceProvider given to this ViewProvider has raised the ItemEndEdit event" +
                    " and has provided a sender that does not implement the interface: IEditableObject.");
            }
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

        public event EventHandler<EventArgs> ItemEndEdit;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnViewSourceRefreshed(string viewName)
        {
            _listHasBeenRefreshed = true;
            Interlocked.CompareExchange(ref ViewSourceRefreshed, null, null)?.Invoke(this, new ViewRefreshedEventArgs(viewName));
        }

        private void OnItemEndEdit(IEditableObject sender, EventArgs e)
        {
            if (ItemEndEdit != null)
            {
                int cnt = ItemEndEdit.GetInvocationList().Length;
                if (cnt > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"There are {cnt} subscribers to this ViewProvider's ItemEndEdit event.");
                }
            }

            Interlocked.CompareExchange(ref ItemEndEdit, null, null)?.Invoke(sender, e);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Interlocked.CompareExchange(ref CollectionChanged, null, null)?.Invoke(sender, e);
        }


        #endregion

        #region Public Properties and Methods

        public DataSourceProvider DataSourceProvider { get; }

        private bool _listHasBeenRefreshed = false;

        private ListCollectionView _view = null;
        public ListCollectionView View
        {
            get
            {
                if(_listHasBeenRefreshed || _view == null) 
                {
                    _view = GetView(_viewSource, _view);
                    _listHasBeenRefreshed = false;
                }
     
                return _view;
            }
        }

        private ListCollectionView GetView(CollectionViewSource cvs, ListCollectionView previousView)
        {
            if (previousView != null)
            {
                ((ICollectionView)previousView).CollectionChanged -= ViewProvider_CollectionChanged;
            }

            if (TryGetListCollectionView(cvs.View, out ListCollectionView lcv))
            {
                if(lcv != null)
                {
                    ((ICollectionView)lcv).CollectionChanged += ViewProvider_CollectionChanged;
                }
                return lcv;
            }
            else
            {
                throw new InvalidOperationException("The view provided by this CollectionViewSource does not implement the ListCollectionView interface.");
            }
        }

        private void ViewProvider_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("This ViewProvider received a collection changed event.");
            OnCollectionChanged(sender, e);
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
