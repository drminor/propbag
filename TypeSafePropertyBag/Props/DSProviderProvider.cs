using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalType = IPropStoreAccessServiceInternal<UInt32, String>;

    // TODO: Implement INotifyPropertyChanged
    internal class DSProviderProvider : IProvideADataSourceProvider 
    {
        #region Private Properties

        PropKindEnum _propKind;
        PropIdType _propId;
        PSAccessServiceInternalType _storeAccessor;

        #endregion

        #region Constructor

        public DSProviderProvider(PropIdType propId, PropKindEnum propKind, PSAccessServiceInternalType storeAccesor)
        {
            _propKind = propKind;
            
            if(! (_propKind == PropKindEnum.ObservableCollection/* || _propKind == PropKindEnum.ObservableCollectionFB*/))
            {
                throw new NotSupportedException($"This IProvideADataSourceProvider {nameof(DSProviderProvider)} cannot create a DataSourceProvider for Prop Items with Kind = {_propKind}.");
            }


            _isReadOnly = false;
            _propId = propId;
            _storeAccessor = storeAccesor;
        }

        #endregion

        #region Public Properties

        
        DataSourceProvider _dataProvider; // Used to be declared as type PBCollectionDataProvider
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
            set
            {
                //if(TryGetPBCollectionDataProvider(value, out PBCollectionDataProvider pbCollectionDSP))
                //{
                //    _dataProvider = pbCollectionDSP;
                //}
                //else
                //{
                //    throw new InvalidOperationException($"The source value must derive from the {nameof(DataSourceProvider)} class.");
                //}

                if (ReferenceEquals(_dataProvider, value))
                {
                    _dataProvider = value;
                }
            }
        }

        public bool IsCollection() => true;

        bool _isReadOnly;
        public bool IsReadOnly() => _isReadOnly;

        #endregion

        #region Private Methods

        private bool TryGetPBCollectionDataProvider(DataSourceProvider dsp, out PBCollectionDataProvider pbCollectionDSP)
        {
            if (dsp == null || dsp is PBCollectionDataProvider)
            {
                pbCollectionDSP = dsp as PBCollectionDataProvider;
                return true;
            }
            else
            {
                pbCollectionDSP = null;
                return false;
            }
        }

        #endregion
    }
}
