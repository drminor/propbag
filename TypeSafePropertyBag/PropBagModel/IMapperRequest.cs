using System;

namespace DRM.TypeSafePropertyBag
{
    using PropModelType = IPropModel<String>;

    public interface IMapperRequest : IEquatable<IMapperRequest>, ICloneable
    {
        Type SourceType { get; set; }

        string ConfigPackageName { get; set; }

        string PropModelFullClassName { get; set; }

        PropModelType PropModel { get; set; }
    }
}