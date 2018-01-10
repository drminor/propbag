using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvideADataSourceProvider
    {
        DataSourceProvider DataSourceProvider { get; } 
        bool IsCollection(); 
        bool IsReadOnly();
    }
}
