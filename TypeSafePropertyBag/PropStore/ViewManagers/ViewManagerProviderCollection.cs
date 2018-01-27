using System;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag
{
    internal class ViewManagerProviderCollection
    {
        #region Private Members

        ConcurrentDictionary<IViewManagerProviderKey, IProvideACViewManager> _dict;

        #endregion

        #region Constructor

        public ViewManagerProviderCollection()
        {
            // TODO: Provide Expected Currency Levels.
            _dict = new ConcurrentDictionary<IViewManagerProviderKey, IProvideACViewManager>();
        }

        #endregion

        #region Public Members

        public int Count => _dict.Count;

        public IProvideACViewManager GetOrAdd(IViewManagerProviderKey viewManagerProviderKey, Func<IViewManagerProviderKey, IProvideACViewManager> vFactory)
        {
            IProvideACViewManager result = _dict.GetOrAdd(viewManagerProviderKey, vFactory);
            return result;
        }

        public bool TryGetValue(IViewManagerProviderKey viewManagerProviderKey, out IProvideACViewManager cViewManagerProvider)
        {
            if(_dict.TryGetValue(viewManagerProviderKey, out cViewManagerProvider))
            {
                return true;
            }
            else
            {
                cViewManagerProvider = null;
                return false;
            }
        }

        public IProvideACViewManager this[IViewManagerProviderKey viewManagerProviderKey]
        {
            get
            {
                IProvideACViewManager result = _dict[viewManagerProviderKey];
                return result;
            }
        }

        #endregion
    }
}
