namespace PropBagTestApp.Infra.Unused
{
    public class DataContextProvider
    {
        public object Data { get; }

        public object GetData()
        {
            return Data;
        }

        public DataContextProvider(string resourceKey)
        {
            Data = JustSayNo.ViewModelHelper.GetNewViewModel(resourceKey);
        }
    }
}
