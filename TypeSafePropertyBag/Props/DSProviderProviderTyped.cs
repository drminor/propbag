using System;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalType = IPropStoreAccessServiceInternal<UInt32, String>;

    internal class DSProviderProviderTyped<T> : IProvideADataSourceProvider<T>
    {
        #region Private Properties

        PropKindEnum _propKind;
        PropIdType _propId;
        PSAccessServiceInternalType _storeAccessor;

        #endregion

        #region Constructor

        public DSProviderProviderTyped(PropKindEnum propKind, PropIdType propId, PSAccessServiceInternalType storeAccesor)
        {
            _propKind = propKind;
            
            if(! (_propKind == PropKindEnum.ObservableCollection/* || _propKind == PropKindEnum.ObservableCollectionFB*/))
            {
                throw new NotSupportedException($"This IProvideADataSourceProvider {nameof(DSProviderProviderTyped<T>)} cannot create a DataSourceProvider for Prop Items with Kind = {_propKind}.");
            }

            IsCollection = true;
            IsReadOnly = false;
            _propId = propId;
            _storeAccessor = storeAccesor;
        }

        #endregion

        #region Public Properties

        PBCollectionDataProvider _dataProvider;
        public DataSourceProvider DataSourceProvider
        {
            get
            {
                if(_dataProvider == null)
                {
                    _dataProvider = new PBCollectionDataProvider(_storeAccessor, _propId);
                }
                return _dataProvider;
            }
        }

        public bool IsCollection { get; }
        public bool IsReadOnly { get; }

        #endregion
    }
}
