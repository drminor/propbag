using System.Windows.Data;

namespace DRM.TypeSafePropertyBag.Unused
{
    public interface IProvideADataSourceProvider
    {
        DataSourceProvider DataSourceProvider { get; }
        bool IsCollection();
        bool IsReadOnly();
    }
}
