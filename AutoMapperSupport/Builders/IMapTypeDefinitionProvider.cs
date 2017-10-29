using DRM.PropBag.ControlModel;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapTypeDefinitionProvider
    {
        IMapTypeDefinition<T> GetTypeDescription<T>(PropModel propModel, Type typeToWrap, string className);
    }
}
