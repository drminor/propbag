using System;

namespace Swhp.AutoMapperSupport
{
    public interface IMapTypeDefinitionProvider
    {
        IMapTypeDefinition GetTypeDescription
        (
            Type targetType,
            object uniqueRef,
            string uniqueToken
        );
    }
}
