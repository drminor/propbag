
namespace Swhp.AutoMapperSupport
{
    public abstract class AutoMapperConfigDetailsBase : IAutoMapperConfigDetails
    {
        protected abstract int _extensionSourceId { get; }

        public AutoMapperConfigDetailsBase(string packageName)
        {
            PackageName = packageName;
        }

        public string PackageName { get; }
        public int ExtensionSourceId => _extensionSourceId;

        public abstract object PayLoad { get; }

    }
}
