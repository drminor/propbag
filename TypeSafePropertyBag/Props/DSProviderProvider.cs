using System;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceInternalType = IPropStoreAccessServiceInternal<UInt32, String>;
    using PSCloneServiceType = IProvidePropStoreCloneService<UInt32, String>;

    internal class DSProviderProvider<T> : IProvideADataSourceProvider<T>
    {
        #region Private Properties

        PropKindEnum _propKind;
        PropIdType _propId;
        PSAccessServiceInternalType _storeAccessor;

        #endregion

        #region Constructor

        public DSProviderProvider(PropKindEnum propKind, PropIdType propId, PSAccessServiceInternalType storeAccesor)
        {
            _propKind = propKind;
            
            if(! (_propKind == PropKindEnum.ObservableCollection || _propKind == PropKindEnum.ObservableCollectionFB))
            {
                throw new NotSupportedException($"This IProvideADataSourceProvider {nameof(DSProviderProvider<T>)} cannot create a DataSourceProvider for Prop Items with Kind = {_propKind}.");
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
