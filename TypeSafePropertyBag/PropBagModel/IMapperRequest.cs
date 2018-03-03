using System;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public interface IMapperRequest : IEquatable<IMapperRequest>, ICloneable
    {
        string ConfigPackageName { get; set; }
        PropModelType PropModel { get; set; }
        string PropModelResourceKey { get; set; }
        Type SourceType { get; set; }
    }
}