using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag
{
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
