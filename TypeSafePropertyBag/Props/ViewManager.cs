using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    public class ViewManager : IManageCViews
    {
        #region Private Members

        private readonly CViewProviderCreator _viewBuilder;

        #endregion

        #region Constructor

        public ViewManager(IProvideADataSourceProvider dataSourceProviderProvider, CViewProviderCreator viewBuilder)
        {
            DataSourceProviderProvider = dataSourceProviderProvider;
            _viewBuilder = viewBuilder;

            //dataSourceProviderProvider.DataSourceProvider.DataChanged += DataSourceProvider_DataChanged;
        }

        #endregion

        #region Public Properties and Methods

        public IProvideADataSourceProvider DataSourceProviderProvider { get; }

        public bool TryGetNewItem(out object newItem)
        {
            if(DataSourceProviderProvider.DataSourceProvider is ISupplyNewItem isni)
            {
                if(isni.TryGetNewItem(out newItem))
                {
                    return true;
                }
                else
                {
                    newItem = null;
                    return false;
                }
            }
            else
            {
                newItem = null;
                return false;
            }
        }

        public object GetNewItem()
        {
            if(TryGetNewItem(out object ni))
            {
                return ni;
            }
            else
            {
                return null;
            }
        }

        // TODO: Note if we were to use a Typed DataSourceProviderProvider we could use it to supply a value of type CT.
        public IList Data
        {
            get
            {
                object test = DataSourceProviderProvider.DataSourceProvider?.Data;
                if (test == null)
                {
                    return null;
                }

                if (test is IList lst)
                {
                    return lst;
                }
                else
                {
                    throw new InvalidOperationException("The data from our DataSourceProvider has a value, but that value does not implement the IList interface.");
                }
            }
        }

        public DataSourceProvider DataSourceProvider => DataSourceProviderProvider.DataSourceProvider;

        public ICollectionView GetDefaultCollectionView()
        {
            ICollectionView result = DefaultView.View;
            return result;
        }

        public ICollectionView GetCollectionView(string viewName)
        {
            IProvideAView viewProvider = this[viewName];
            ICollectionView result = viewProvider.View;
            return result;
        }

        public IProvideAView GetDefaultViewProvider()
        {
            return DefaultView;
        }

        public IProvideAView GetViewProvider(string viewName)
        {
            IProvideAView result = this[viewName];
            return result;
        }

        public void SetDefaultViewProvider(IProvideAView value)
        {
            DefaultView = value;
        }

        public void SetViewProvider(IProvideAView value, string viewName)
        {
            this[viewName] = value;
        }

        public void Refresh()
        {
           DataSourceProvider.Refresh();
        }

        #endregion

        #region View Management

        IProvideAView _defaultView;
        IProvideAView DefaultView
        {
            get
            {
                if(_defaultView == null)
                {
                    string viewName = null;
                    _defaultView = _viewBuilder(viewName, DataSourceProviderProvider.DataSourceProvider); 
                }
                return _defaultView;
            }
            set
            {
                _defaultView = value;
            }
        }

        public bool IsDataSourceReadOnly() => DataSourceProviderProvider.IsReadOnly();
        public bool IsGetNewItemSupported => DataSourceProviderProvider is ISupplyNewItem;

        IDictionary<string, IProvideAView> _views;
        public IProvideAView this[string viewName]
        {
            get
            {
                if (viewName == null) throw new ArgumentNullException(viewName, "The view's name must have a value.");

                if (_views == null)
                {
                    _views = new Dictionary<string, IProvideAView>();
                }
                else
                {
                    if (_views.TryGetValue(viewName, out IProvideAView theViewProvider))
                    {
                        return theViewProvider;
                    }
                }

                IProvideAView vp = _viewBuilder(viewName, DataSourceProviderProvider.DataSourceProvider);
                _views.Add(viewName, vp);
                return vp;
            }
            private set
            {
                if (viewName == null) throw new ArgumentNullException(viewName, "The view's name must have a value.");

                if (_views == null)
                {
                    _views = new Dictionary<string, IProvideAView>
                    {
                        { viewName, value }
                    };
                }
                else
                {
                    if(_views.ContainsKey(viewName))
                    {
                        _views[viewName] = value;
                    }
                    else
                    {
                        _views.Add(viewName, value);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        //private bool TryGetDataSourceProvider(object source, out DataSourceProvider dsp)
        //{
        //    if (source == null)
        //    {
        //        dsp = null;
        //        return true;
        //    }

        //    if (source is DataSourceProvider dspTest)
        //    {
        //        dsp = dspTest;
        //        return true;
        //    }
        //    else
        //    {
        //        dsp = null;
        //        return false;
        //    }
        //}

        //private bool TryGetListCollectionView(ICollectionView icv, out ListCollectionView lcv)
        //{
        //    if (icv == null)
        //    {
        //        lcv = null;
        //        return true;
        //    }

        //    if (icv is ListCollectionView lcvTest)
        //    {
        //        lcv = lcvTest;
        //        return true;
        //    }
        //    else
        //    {
        //        lcv = null;
        //        return false;
        //    }
        //}

        #endregion
    }
}
