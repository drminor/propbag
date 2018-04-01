using DRM.PropBag.AutoMapperSupport;
using System;

namespace DRM.TypeSafePropertyBag
{
    using static DRM.TypeSafePropertyBag.TypeExtensions.TypeExtensions;

    public class SimpleMapTypeDefinitionProvider : IMapTypeDefinitionProvider
    {
        public IMapTypeDefinition GetTypeDescription(object propModel,Type targetType, object propFactory, string fullClassName)
        {
            if (targetType.IsPropBagBased())
            {
                return new MapTypeDefinition(propModel, targetType, propFactory, fullClassName);
            }
            else
            {
                return new MapTypeDefinition(targetType);
            }
        }
    }
}
