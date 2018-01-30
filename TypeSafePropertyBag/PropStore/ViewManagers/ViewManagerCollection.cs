using System;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    internal class ViewManagerCollection
    {
        #region Private Members

        ConcurrentDictionary<PropIdType, IManageCViews> _dict;


        #endregion

        #region Constructor

        public ViewManagerCollection()
        {
            // TODO: Provide Expected Currency Levels.
            _dict = new ConcurrentDictionary<PropIdType, IManageCViews>();
        }

        #endregion

        #region Public Members

        public int Count => _dict.Count;

        public IManageCViews GetOrAdd(PropIdType propId, Func<PropIdType, IManageCViews> vFactory)
        {
            IManageCViews result = _dict.GetOrAdd(propId, vFactory);
            return result;
        }

        public bool TryGetValue(PropIdType propId, out IManageCViews cViewManager)
        {
            if(_dict.TryGetValue(propId, out cViewManager))
            {
                return true;
            }
            else
            {
                cViewManager = null;
                return false;
            }
        }

        public IManageCViews this[PropIdType propId]
        {
            get
            {
                IManageCViews result = _dict[propId];
                return result;
            }
        }

        #endregion

    }
}
