using System;
using System.Threading;

namespace PropBagTestApp.Infra
{
    public class ViewModelProvider
    {
        private Lazy<object> _dataGetter;
        //static private object _dataHold;

        public ViewModelProvider(string resourceKey)
        {
            ResourceKey = resourceKey;

            _dataGetter = new Lazy<object>
            (
                () => PropStoreServicesForThisApp.ViewModelFactory.GetNewViewModel(GetResourceKeyWithSuffix(ResourceKey, PropStoreServicesForThisApp.ConfigPackageNameSuffix)),
                    LazyThreadSafetyMode.PublicationOnly
            );
        }

        public string ResourceKey { get; }

        public object Data
        {
            get
            {
                //_dataHold = _dataGetter.Value;
                return _dataGetter.Value;
            }
        }

        public object GetData() => Data;

        private string GetResourceKeyWithSuffix(string rawKey, string suffix)
        {
            string result = suffix != null ? $"{rawKey}_{suffix}" : rawKey;
            return result;
        }

    }

    //public class ViewModelProvider
    //{
    //    private Lazy<object> _dataGetter;

    //    public ViewModelProvider(string resourceKey)
    //    {
    //        ResourceKey = resourceKey;

    //        _dataGetter = new Lazy<object>
    //        (
    //            () => PropStoreServicesForThisApp.ViewModelHelper.GetNewViewModel(ResourceKey),
    //            LazyThreadSafetyMode.PublicationOnly
    //        );
    //    }

    //    public string ResourceKey { get; }
    //    public object Data => _dataGetter.Value;
    //    public object GetData() => Data;
    //}
}
