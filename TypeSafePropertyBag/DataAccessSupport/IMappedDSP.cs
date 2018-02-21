using System;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    // TODO: in order for this to be used, it must include all of the members present in the DataSourceProvider abstract class.

    public interface IMappedDSP_Unused<T> : IDisposable, INotifyItemEndEdit, IProvideADataSourceProvider, IHaveACrudWithMapping<T> where T : INotifyItemEndEdit
    {
    }
}
