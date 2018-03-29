using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public class SimpleMapTypeDefinitionProvider : IMapTypeDefinitionProvider
    {
        public IMapTypeDefinition<T> GetTypeDescription<T>(PropModelType propModel/*, Type typeToWrap*/, IPropFactory propFactory, string fullClassName)
        {
            if (typeof(T).IsPropBagBased())
            {
                return new MapTypeDefinition<T>(propModel/*, typeToWrap*/, fullClassName, propFactory);
            }
            else
            {
                return new MapTypeDefinition<T>(typeof(T).FullName);
            }
        }
    }
}
