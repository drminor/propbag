using System;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalType = IPropStoreAccessServiceInternal<UInt32, String>;

    // TODO: Implement INotifyPropertyChanged
    internal class DSProviderProvider : IProvideADataSourceProvider, INotifyItemEndEdit
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

        public event EventHandler<EventArgs> ItemEndEdit
        {
            add
            {
                ((INotifyItemEndEdit)_dataProvider).ItemEndEdit += value;
            }

            remove
            {
                ((INotifyItemEndEdit)_dataProvider).ItemEndEdit -= value;
            }
        }

        #endregion

        #region Public Properties


        PBCollectionDataProvider _dataProvider; // Used to be declared as type PBCollectionDataProvider
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
                if (TryGetPBCollectionDataProvider(value, out PBCollectionDataProvider pbCollectionDSP))
                {
                    _dataProvider = pbCollectionDSP;
                }
                else
                {
                    throw new InvalidOperationException($"The source value must derive from the {nameof(DataSourceProvider)} class.");
                }

                //if (ReferenceEquals(_dataProvider, value))
                //{
                //    _dataProvider = value;
                //}
            }
        }

        private void DataProvider_ItemEndEdit(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool IsCollection() => true;

        bool _isReadOnly;
        public bool IsReadOnly() => _isReadOnly;

        #endregion

        #region Private Methods

        private bool TryGetPBCollectionDataProvider(DataSourceProvider dsp, out PBCollectionDataProvider pbCollectionDSP)
        {
            if (dsp is PBCollectionDataProvider pbDSP)
            {
                pbCollectionDSP = pbDSP;
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
