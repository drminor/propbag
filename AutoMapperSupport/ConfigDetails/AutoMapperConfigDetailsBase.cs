
namespace Swhp.AutoMapperSupport
{
    public abstract class AutoMapperConfigDetailsBase : IAutoMapperConfigDetails
    {
        public AutoMapperConfigDetailsBase(int extensionSourceId, string packageName)
        {
            ExtensionSourceId = extensionSourceId;
            PackageName = packageName;
        }

        public int ExtensionSourceId { get; }
        public string PackageName { get; }

        public abstract object Payload { get; }
    }
}
