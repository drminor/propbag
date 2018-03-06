using System;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public interface IMapperRequest : IEquatable<IMapperRequest>, ICloneable
    {
        Type SourceType { get; set; }

        string ConfigPackageName { get; set; }

        string PropModelResourceKey { get; set; }
        PropModelType PropModel { get; set; }
    }
}