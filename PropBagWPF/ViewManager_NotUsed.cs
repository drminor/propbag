using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

using System.Windows.Data;

namespace DRM.PropBagWPF.Unused
{
    // This is not being used.
    public class ViewManager<CT> : IManageCViews<CT>  where CT : class, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Private Members

        private readonly IProvideADataSourceProvider _dataSourceProviderProvider;

        #endregion

        #region Constructor

        public ViewManager(IProvideADataSourceProvider dataSourceProviderProvider)
        {
            _dataSourceProviderProvider = dataSourceProviderProvider;
        }

        #endregion

        #region Public Properties and Methods


        public CT Data
        {
            get
            {
                object test = _dataSourceProviderProvider.DataSourceProvider?.Data;
                if (test == null)
                {
                    return null;
                }

                if(test is IList && test is IEnumerable && test is INotifyCollectionChanged && test is INotifyPropertyChanged)
                {
                    CT result = test as CT;
                    return result;
                }
                else
                {
                    throw new InvalidOperationException("The data from our DataSourceProvider has a value, but that value does not implement all of the following interfaces: IList, INotifyCollectionChanged and INotifyPropertyChanged.");
                }
            }
        }

        public DataSourceProvider DataSourceProvider => _dataSourceProviderProvider.DataSourceProvider;

        public ICollectionView GetDefaultCollectionView()
        {
            ICollectionView result = DefaultView.View;
            return result;
        }

        public void SetDefaultCollectionView(ICollectionView value)
        {
            throw new NotImplementedException();
        }

        public ICollectionView GetCollectionView(string key)
        {
            IProvideAView viewProvider = this[key];
            ICollectionView result = viewProvider.View;
            return result;
        }

        public void SetCollectionView(string key, ICollectionView value)
        {
            throw new NotImplementedException();
        }

        public IProvideAView GetViewProvider()
        {
            return DefaultView;
        }

        public IProvideAView GetViewProvider(string key)
        {
            IProvideAView result = this[key];
            return result;
        }

        public bool IsCollection() => _dataSourceProviderProvider.IsCollection();

        public bool IsReadOnly() => _dataSourceProviderProvider.IsReadOnly();

        #endregion

        #region View Management

        IProvideAView _defaultView;
        IProvideAView DefaultView
        {
            get
            {
                if(_defaultView == null)
                {
                    _defaultView = new ViewProvider(null, _dataSourceProviderProvider.DataSourceProvider);
                }
                return _defaultView;
            }
        }

        IDictionary<string, IProvideAView> _views;
        public IProvideAView this[string key]
        {
            get
            {
                if (_views == null)
                {
                    _views = new Dictionary<string, IProvideAView>();
                }
                else
                {
                    if (_views.TryGetValue(key, out IProvideAView theViewProvider))
                    {
                        return theViewProvider;
                    }
                }

                ViewProvider vp = new ViewProvider(key, _dataSourceProviderProvider.DataSourceProvider);
                _views.Add(key, vp);
                return vp;
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
