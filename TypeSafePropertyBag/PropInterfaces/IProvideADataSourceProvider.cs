using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvideADataSourceProvider<T> : IProvideADataSourceProviderGen
    {

    }

    public interface IProvideADataSourceProviderGen
    {
        DataSourceProvider DataSourceProvider { get; }
        bool IsCollection { get; }
        bool IsReadOnly { get; }
    }
}
