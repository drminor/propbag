using System;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PSAccessServiceInternalInterface = IPropStoreAccessServiceInternal<UInt32, String>;

    // TODO: Implement INotifyPropertyChanged
    public class PBCollectionDSP_Provider : IProvideADataSourceProvider//, INotifyItemEndEdit
    {
        #region Private Properties

        PropKindEnum _propKind;
        //PropIdType _propId;
        //PSAccessServiceInternalInterface _storeAccessor;
        IWatchAPropItemGen _propItemWatcherGen;

        #endregion

        #region Constructor

        public PBCollectionDSP_Provider(PropKindEnum propKind, IWatchAPropItemGen propItemWatcherGen)
        {
            _propKind = propKind;
            
            if(! (_propKind == PropKindEnum.ObservableCollection/* || _propKind == PropKindEnum.ObservableCollectionFB*/))
            {
                throw new NotSupportedException($"This IProvideADataSourceProvider {nameof(PBCollectionDSP_Provider)} cannot create a DataSourceProvider for Prop Items with Kind = {_propKind}.");
            }

            _isReadOnly = false;
            //_propId = propId;
            //_storeAccessor = storeAccesor;
            _propItemWatcherGen = propItemWatcherGen;
        }

        //public event EventHandler<EventArgs> ItemEndEdit
        //{
        //    add
        //    {
        //        ((INotifyItemEndEdit)_dataProvider).ItemEndEdit += value;
        //    }

        //    remove
        //    {
        //        ((INotifyItemEndEdit)_dataProvider).ItemEndEdit -= value;
        //    }
        //}

        #endregion

        #region Public Properties


        PBCollectionDSP _dataProvider; 
        public DataSourceProvider DataSourceProvider
        {
            get
            {
                if(_dataProvider == null)
                {
                    //IWatchAPropItemGen propItemWatcherGen = new PropItemWatcherGen(_storeAccessor, _propId);
                    _dataProvider = new PBCollectionDSP(_propItemWatcherGen);
                }
                return _dataProvider;
            }
            //set
            //{
            //    if (!ReferenceEquals(_dataProvider, value))
            //    {
            //        if (TryGetPBCollectionDataProvider(value, out PBCollectionDataProvider pbCollectionDSP))
            //        {
            //            _dataProvider = pbCollectionDSP;
            //        }
            //        else
            //        {
            //            throw new InvalidOperationException($"The source value must be, or derive from, the {nameof(PBCollectionDataProvider)} class.");
            //        }
            //    }
            //}
        }

        //private void DataProvider_ItemEndEdit(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        public bool IsCollection() => true;

        bool _isReadOnly;
        public bool IsReadOnly() => _isReadOnly;

        #endregion

        #region Private Methods

        private bool TryGetPBCollectionDataProvider(DataSourceProvider dsp, out PBCollectionDSP pbCollectionDSP)
        {
            if (dsp is PBCollectionDSP pbDSP)
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
