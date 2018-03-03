using System;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;

    public interface IPropModelItem : IDisposable, ICloneable
    {
        string PropertyName { get; set; }
        Type PropertyType { get; set; }
        PropKindEnum PropKind { get; set; }
        ITypeInfoField PropTypeInfoField { get; set; }
        PropStorageStrategyEnum StorageStrategy { get; set; }
        bool TypeIsSolid { get; set; }

        IPropBinderField BinderField { get; set; }
        Type CollectionType { get; set; }
        IPropComparerField ComparerField { get; set; }
        IPropDoWhenChangedField DoWhenChangedField { get; set; }
        object ExtraInfo { get; set; }
        IPropInitialValueField InitialValueField { get; set; }
        Type ItemType { get; set; }

        string MapperRequestResourceKey { get; set; }
        IMapperRequest MapperRequest { get; set; }

        // These are only available after some Prop has been created from this PropModelItem.
        IPropTemplate PropTemplate { get; set; }
        Func<PropNameType, object, bool, IPropTemplate, IProp> PropCreator { get; set; }
        object InitialValueCooked { get; set; }
    }
}