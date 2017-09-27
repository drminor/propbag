using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRM.PropBag.ControlModel;

using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public class PropBagMapperKey<TSource, TDestination> : IPropBagMapperKey<TSource, TDestination>
    {
        public PropBagMapperKey(PropModel pm, Type baseType, bool useCustom = false, Func<TDestination, TSource> constructSourceFunc = null)
        {
            if (typeof(TSource) is IPropBag) throw new ApplicationException("The first type, TSource, is expected to be a regular, non-propbag-based type.");
            if (typeof(TDestination) is IPropBag) throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");

            SourceTypeDef = GetTypeDef<TSource>(pm, baseType);
            DestinationTypeDef = GetTypeDef<TDestination>(pm, baseType);

            ConstructSourceFunc = constructSourceFunc;
            ConstructDestinationFunc = null;

            UseCustom = useCustom;
            if(useCustom)
            {
                CreateMapper = (untypedKey) =>
                {
                    IPropBagMapperKey<TSource, TDestination> typedKey = (IPropBagMapperKey<TSource, TDestination>)untypedKey;
                    return new PropBagMapperCustom<TSource, TDestination>(typedKey);
                };
            }
            else
            {
                CreateMapper = (untypedKey) =>
                {
                    IPropBagMapperKey<TSource, TDestination> typedKey = (IPropBagMapperKey<TSource, TDestination>)untypedKey;
                    return new PropBagMapper<TSource, TDestination>(typedKey);
                };
            }
          
        }

        private IMapTypeDefinition<T> GetTypeDef<T>(PropModel pm, Type baseType)
        {
            if(IsPropGenBased(typeof(T)))
            {
                return new MapTypeDefinition<T>(pm, baseType);
            }
            else
            {
                return new MapTypeDefinition<T>();
            }
        }

        private bool IsPropGenBased(Type t)
        {
            //IEnumerable<Type> r = t.GetInterfaces();
            //Type a = t.GetInterfaces().FirstOrDefault(x => x.Name == "IPropBag");

            return null != t.GetInterfaces().FirstOrDefault(x => x.Name == "IPropBag");
        }

        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> CreateMapper { get; }

        public IMapTypeDefinition<TSource> SourceTypeDef { get; }

        public IMapTypeDefinition<TDestination> DestinationTypeDef { get; }

        public Func<TDestination, TSource> ConstructSourceFunc { get; }

        public Func<TSource, TDestination> ConstructDestinationFunc { get; }

        public bool UseCustom { get; set; }

        public IMapTypeDefinitionGen SourceTypeGenDef => SourceTypeDef as IMapTypeDefinitionGen;

        public IMapTypeDefinitionGen DestinationTypeGenDef => DestinationTypeDef as IMapTypeDefinitionGen;


    }
}
