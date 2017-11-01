using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapTypeDefinitionProvider
    {
        IMapTypeDefinition<T> GetTypeDescription<T>(PropModel propModel, /*IPropFactory propFactory, */Type typeToWrap, string className);
    }
}
