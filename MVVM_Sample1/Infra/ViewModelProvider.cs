using System;
using System.Threading;

namespace MVVM_Sample1.Infra
{
    public class ViewModelProvider
    {
        private Lazy<object> _dataGetter;

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
}
