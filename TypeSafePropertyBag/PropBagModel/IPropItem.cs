using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropItem
    {
        IPropBinderField BinderField { get; set; }
        Type CollectionType { get; set; }
        IPropComparerField ComparerField { get; set; }
        IPropDoWhenChangedField DoWhenChangedField { get; set; }
        string ExtraInfo { get; set; }
        IPropInitialValueField InitialValueField { get; set; }
        Type ItemType { get; set; }
        IMapperRequest MapperRequest { get; set; }
        string PropertyName { get; set; }
        Type PropertyType { get; set; }
        PropKindEnum PropKind { get; set; }
        ITypeInfoField PropTypeInfoField { get; set; }
        PropStorageStrategyEnum StorageStrategy { get; set; }
        bool TypeIsSolid { get; set; }
    }
}