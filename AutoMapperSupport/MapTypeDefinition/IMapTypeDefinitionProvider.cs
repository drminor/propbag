using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapTypeDefinitionProvider
    {
        IMapTypeDefinition<T> GetTypeDescription<T>(IPropModel propModel, Type typeToWrap, string className, IPropFactory propFactory);
    }
}
