using System;
using System.Threading;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.Infra
{
    public class ViewModelProvider
    {
        private Lazy<object> _dataGetter;

        public ViewModelProvider(string resourceKey)
        {
            ResourceKey = resourceKey;

            _dataGetter = new Lazy<object>
            (
                // TODO:A-FixMe
                () => new object()
                //() => PropStoreServicesForThisApp.ViewModelHelper.GetNewViewModel(ResourceKey),
                //LazyThreadSafetyMode.PublicationOnly
            );
        }

        public string ResourceKey { get; }
        public object Data => _dataGetter.Value;
        public object GetData() => Data;
    }
}
