using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimpleMapTypeDefinitionProvider : IMapTypeDefinitionProvider
    {
        public IMapTypeDefinition<T> GetTypeDescription<T>(IPropModel propModel, Type typeToWrap, string fullClassName, IPropFactory propFactory)
        {
            if (typeof(T).IsPropBagBased())
            {
                // TODO: Honor the DeriveFromClassMode specified in the PropModel.
                return new MapTypeDefinition<T>(propModel, typeToWrap, fullClassName, propFactory);
            }
            else
            {
                return new MapTypeDefinition<T>();
            }
        }
    }
}
