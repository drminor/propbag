using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimpleMapTypeDefinitionProvider : IMapTypeDefinitionProvider
    {
        public IMapTypeDefinition<T> GetTypeDescription<T>(PropModel propModel, Type typeToWrap, string className, IPropFactory propFactory)
        {
            if (typeof(T).IsPropBagBased())
            {
                return new MapTypeDefinition<T>(propModel, propFactory, typeToWrap);
            }
            else
            {
                return new MapTypeDefinition<T>();
            }
        }
    }
}
