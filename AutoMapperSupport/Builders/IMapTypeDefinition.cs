using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapTypeDefinition<T> : IMapTypeDefinitionGen
    {
    }

    // ToDo: Document these members.
    public interface IMapTypeDefinitionGen : IEquatable<IMapTypeDefinitionGen>
    {
        Type Type { get; }

        bool IsPropBag { get; }
        PropModel PropModel { get; }
        Type NewWrapperType { get; }
        IPropFactory PropFactory { get; }

    }
}
