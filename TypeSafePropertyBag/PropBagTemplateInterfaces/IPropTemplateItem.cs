using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropTemplateItem
    {
        string ExtraInfo { get; set; }
        string PropertyName { get; set; }
        Type PropertyType { get; set; }
        PropKindEnum PropKind { get; set; }
        PropStorageStrategyEnum StorageStrategy { get; set; }
        bool TypeIsSolid { get; set; }


    }
}