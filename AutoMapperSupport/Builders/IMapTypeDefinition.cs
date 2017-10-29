using DRM.PropBag.ControlModel;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapTypeDefinition<T> : IMapTypeDefinitionGen
    {
    }

    public interface IMapTypeDefinitionGen : IEquatable<IMapTypeDefinitionGen>
    {
        Type Type { get; }

        bool IsPropBag { get; }
        PropModel PropModel { get; }
        Type NewWrapperType { get; }
        //IPropFactory PropFactory { get; }

    }
}
