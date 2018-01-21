namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    public interface IDoCrudWithMapping<TDestination> : ISupplyNewItem
    {
        TDestination GetNewItem();
        object GetUnmappedItem(TDestination mappedItem);
    }

    public interface IHaveACrudWithMapping<TDestination> : ISupplyNewItem
    {
        IDoCrudWithMapping<TDestination> CrudWithMapping { get; }
    }

    public interface ISupplyNewItem
    {
        bool TryGetNewItem(out object newItem);
    }

}