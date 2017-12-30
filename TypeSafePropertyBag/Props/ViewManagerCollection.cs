using System;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    internal class ViewManagerCollection
    {
        #region Private Members

        ConcurrentDictionary<IPropData, IManageCViews> _dict;

        Func<IPropData, IManageCViews> _factory;

        #endregion

        #region Constructor

        public ViewManagerCollection(Func<IPropData, IManageCViews> factory)
        {
            _factory = factory;
            // TODO: Provide Expected Currency Levels.
            _dict = new ConcurrentDictionary<IPropData, IManageCViews>();
        }

        #endregion

        #region Public Members

        public int Count => _dict.Count;

        public IManageCViews GetOrAdd(IPropData sourceCollectionPropItem)
        {
            IManageCViews result = _dict.GetOrAdd(sourceCollectionPropItem, _factory);
            return result;
        }

        #endregion
    }
}
