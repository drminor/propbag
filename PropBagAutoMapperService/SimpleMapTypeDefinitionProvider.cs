using Swhp.AutoMapperSupport;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using static DRM.TypeSafePropertyBag.TypeExtensions.TypeExtensions;

    public class SimpleMapTypeDefinitionProvider : IMapTypeDefinitionProvider
    {
        public IMapTypeDefinition GetTypeDescription(object propModel,Type targetType, object propFactory, string fullClassName)
        {
            if (targetType.IsPropBagBased())
            {
                return new MapTypeDefinition(targetType, propModel, fullClassName);
            }
            else
            {
                return new MapTypeDefinition(targetType);
            }
        }
    }
}
