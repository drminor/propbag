//using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    //using PropModelType = IPropModel<String>;

    public interface IMapTypeDefinitionProvider
    {
        IMapTypeDefinition GetTypeDescription
            (
            object propModel,
            Type targetType,
            object propFactory,
            string className
            );
    }
}
