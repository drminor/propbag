
namespace Swhp.AutoMapperSupport
{
    public interface IAutoMapperConfigDetails
    {
        int ExtensionSourceId { get; }
        string PackageName { get; }

        object Payload { get; }
    }
}
