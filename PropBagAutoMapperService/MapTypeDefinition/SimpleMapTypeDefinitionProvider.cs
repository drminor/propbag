using Swhp.AutoMapperSupport;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using static DRM.TypeSafePropertyBag.TypeExtensions.TypeExtensions;

    public class SimpleMapTypeDefinitionProvider : IMapTypeDefinitionProvider
    {
        public IMapTypeDefinition GetTypeDescription
            (
            Type targetType,
            object uniqueRef,
            string uniqueToken
            )
        {
            IMapTypeDefinition result;

            bool isPropBagBased = targetType.IsPropBagBased();
            if(isPropBagBased)
            {
                result = new MapTypeDefinition(targetType, isPropBagBased, uniqueRef, uniqueToken);
            }
            else
            {
                result = new MapTypeDefinition(targetType);
            }

            return result;
        }
    }
}
