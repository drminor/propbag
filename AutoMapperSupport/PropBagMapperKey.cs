using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DRM.PropBag.ControlModel;

using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public class PropBagMapperKey<TSource, TDestination> : PropBagMapperKeyGen, IPropBagMapperKey<TSource, TDestination>, IEquatable<IPropBagMapperKey<TSource, TDestination>>, IEquatable<PropBagMapperKey<TSource, TDestination>>
    {
        public IMapTypeDefinition<TSource> SourceTypeDef { get; }

        public IMapTypeDefinition<TDestination> DestinationTypeDef { get; }

        public Func<TDestination, TSource> ConstructSourceFunc { get; }

        public Func<TSource, TDestination> ConstructDestinationFunc { get; }

        public PropBagMapperKey(PropModel pm,
            Type baseType,
            PropBagMappingStrategyEnum mappingStrategy,
            Func<TDestination, TSource> constructSourceFunc = null,
            Func<TSource, TDestination> constructDestinationFunc = null)
            : base(mappingStrategy,
                  GetTypeDef<TSource>(pm, baseType) as IMapTypeDefinitionGen,
                  GetTypeDef<TDestination>(pm, baseType) as IMapTypeDefinitionGen, GetCreaterFunc(mappingStrategy))
        {
            if (typeof(TSource) is IPropBag) throw new ApplicationException("The first type, TSource, is expected to be a regular, non-propbag-based type.");
            if (typeof(TDestination) is IPropBag) throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");

            SourceTypeDef = base.SourceTypeGenDef as IMapTypeDefinition<TSource>; //GetTypeDef<TSource>(pm, baseType);
            DestinationTypeDef = base.DestinationTypeGenDef as IMapTypeDefinition<TDestination>; // GetTypeDef<TDestination>(pm, baseType);

            ConstructSourceFunc = constructSourceFunc;
            ConstructDestinationFunc = constructDestinationFunc;
        }

        private static Func<IPropBagMapperKeyGen, IPropBagMapperGen> GetCreaterFunc(PropBagMappingStrategyEnum mappingStrategy)
        {
            Func<IPropBagMapperKeyGen, IPropBagMapperGen> result;
            switch (mappingStrategy)
            {
                case PropBagMappingStrategyEnum.ExtraMembers:
                    {
                        result = (untypedKey) =>
                        {
                            IPropBagMapperKey<TSource, TDestination> typedKey = (IPropBagMapperKey<TSource, TDestination>)untypedKey;
                            return new PropBagMapperCustom<TSource, TDestination>(typedKey);
                        };
                        break;
                    }
                case PropBagMappingStrategyEnum.EmitProxy:
                    {
                        result = (untypedKey) =>
                        {
                            IPropBagMapperKey<TSource, TDestination> typedKey = (IPropBagMapperKey<TSource, TDestination>)untypedKey;
                            return new PropBagMapper<TSource, TDestination>(typedKey);
                        };
                        break;
                    }
                default:
                    {
                        throw new ApplicationException($"Unsupported, or unexpected value of {nameof(PropBagMappingStrategyEnum)}.");
                    }
            }
            return result;
        }

        private static IMapTypeDefinition<T> GetTypeDef<T>(PropModel pm, Type baseType)
        {
            if(typeof(T).IsPropGenBased())
            {
                return new MapTypeDefinition<T>(pm, baseType);
            }
            else
            {
                return new MapTypeDefinition<T>();
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropBagMapperKeyGen);
        }

        public override int GetHashCode()
        {
            var hashCode = 1780333077;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition<TSource>>.Default.GetHashCode(SourceTypeDef);
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition<TDestination>>.Default.GetHashCode(DestinationTypeDef);
            return hashCode;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        // TODO: Implement Equals for PropBagMapperKey
        public bool Equals(PropBagMapperKey<TSource, TDestination> other)
        {
            throw new NotImplementedException();
        }

        // TODO: Implement Equals for IPropBagMapperKey
        public bool Equals(IPropBagMapperKey<TSource, TDestination> other)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(PropBagMapperKey<TSource, TDestination> key1, PropBagMapperKey<TSource, TDestination> key2)
        {
            return EqualityComparer<PropBagMapperKey<TSource, TDestination>>.Default.Equals(key1, key2);
        }

        public static bool operator !=(PropBagMapperKey<TSource, TDestination> key1, PropBagMapperKey<TSource, TDestination> key2)
        {
            return !(key1 == key2);
        }
    }

    public class PropBagMapperKeyGen : IPropBagMapperKeyGen, IEquatable<IPropBagMapperKeyGen>, IEquatable<PropBagMapperKeyGen>
    {
        public PropBagMappingStrategyEnum MappingStrategy { get; }

        public IMapTypeDefinitionGen SourceTypeGenDef { get; set; }

        public IMapTypeDefinitionGen DestinationTypeGenDef { get; set; }

        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> CreateMapper { get; set; }
        public PropBagMapperKeyGen(PropBagMappingStrategyEnum mappingStrategy,
            IMapTypeDefinitionGen sourceTypeGenDef,
            IMapTypeDefinitionGen destinationTypeGenDef,
            Func<IPropBagMapperKeyGen, IPropBagMapperGen> createMapper)
        {
            MappingStrategy = mappingStrategy;
            SourceTypeGenDef = sourceTypeGenDef;
            DestinationTypeGenDef = destinationTypeGenDef;
            CreateMapper = createMapper;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropBagMapperKeyGen);
        }

        public bool Equals(PropBagMapperKeyGen other)
        {
            return other != null &&
                   MappingStrategy == other.MappingStrategy &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);
        }

        public bool Equals(IPropBagMapperKeyGen other)
        {
            return other != null &&
                   MappingStrategy == other.MappingStrategy &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);
        }

        public override int GetHashCode()
        {
            var hashCode = 1973524047;
            hashCode = hashCode * -1521134295 + MappingStrategy.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinitionGen>.Default.GetHashCode(SourceTypeGenDef);
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinitionGen>.Default.GetHashCode(DestinationTypeGenDef);
            return hashCode;
        }

        public override string ToString()
        {
            return $"PropBagMapperKey: S={SourceTypeGenDef.ToString()}, D={DestinationTypeGenDef.ToString()}";
        }

        public static bool operator ==(PropBagMapperKeyGen gen1, PropBagMapperKeyGen gen2)
        {
            return EqualityComparer<PropBagMapperKeyGen>.Default.Equals(gen1, gen2);
        }

        public static bool operator !=(PropBagMapperKeyGen gen1, PropBagMapperKeyGen gen2)
        {
            return !(gen1 == gen2);
        }
    }

    // Old version, works just as well without explicitly inheriting from class that implements IPropBagMapperKeyGen
    class PropBagMapperKey_OLD<TSource, TDestination> : IPropBagMapperKey<TSource, TDestination>,
        IEquatable<PropBagMapperKey_OLD<TSource, TDestination>>, IEquatable<IPropBagMapperKey<TSource, TDestination>>,
        IEquatable<IPropBagMapperKeyGen>
    {
        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> CreateMapper { get; }

        public IMapTypeDefinition<TSource> SourceTypeDef { get; }

        public IMapTypeDefinition<TDestination> DestinationTypeDef { get; }

        public Func<TDestination, TSource> ConstructSourceFunc { get; }

        public Func<TSource, TDestination> ConstructDestinationFunc { get; }

        public PropBagMappingStrategyEnum MappingStrategy { get; set; }

        public IMapTypeDefinitionGen SourceTypeGenDef => SourceTypeDef as IMapTypeDefinitionGen;

        public IMapTypeDefinitionGen DestinationTypeGenDef => DestinationTypeDef as IMapTypeDefinitionGen;
        public PropBagMapperKey_OLD(PropModel pm,
            Type baseType,
            PropBagMappingStrategyEnum mappingStrategy,
            Func<TDestination, TSource> constructSourceFunc = null,
            Func<TSource, TDestination> constructDestinationFunc = null)
        {
            if (typeof(TSource) is IPropBag) throw new ApplicationException("The first type, TSource, is expected to be a regular, non-propbag-based type.");
            if (typeof(TDestination) is IPropBag) throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");

            SourceTypeDef = GetTypeDef<TSource>(pm, baseType);
            DestinationTypeDef = GetTypeDef<TDestination>(pm, baseType);

            ConstructSourceFunc = constructSourceFunc;
            ConstructDestinationFunc = constructDestinationFunc;

            MappingStrategy = mappingStrategy;

            CreateMapper = GetCreaterFunc(mappingStrategy);
        }

        private static Func<IPropBagMapperKeyGen, IPropBagMapperGen> GetCreaterFunc(PropBagMappingStrategyEnum mappingStrategy)
        {
            Func<IPropBagMapperKeyGen, IPropBagMapperGen> result;
            switch (mappingStrategy)
            {
                case PropBagMappingStrategyEnum.ExtraMembers:
                    {
                        result = (untypedKey) =>
                        {
                            IPropBagMapperKey<TSource, TDestination> typedKey = (IPropBagMapperKey<TSource, TDestination>)untypedKey;
                            return new PropBagMapperCustom<TSource, TDestination>(typedKey);
                        };
                        break;
                    }
                case PropBagMappingStrategyEnum.EmitProxy:
                    {
                        result = (untypedKey) =>
                        {
                            IPropBagMapperKey<TSource, TDestination> typedKey = (IPropBagMapperKey<TSource, TDestination>)untypedKey;
                            return new PropBagMapper<TSource, TDestination>(typedKey);
                        };
                        break;
                    }
                default:
                    {
                        throw new ApplicationException($"Unsupported, or unexpected value of {nameof(PropBagMappingStrategyEnum)}.");
                    }
            }
            return result;
        }

        private static IMapTypeDefinition<T> GetTypeDef<T>(PropModel pm, Type baseType)
        {
            if (IsPropGenBased(typeof(T)))
            {
                return new MapTypeDefinition<T>(pm, baseType);
            }
            else
            {
                return new MapTypeDefinition<T>();
            }
        }

        private static bool IsPropGenBased(Type t)
        {
            //IEnumerable<Type> r = t.GetInterfaces();
            //Type a = t.GetInterfaces().FirstOrDefault(x => x.Name == "IPropBag");

            return null != t.GetInterfaces().FirstOrDefault(x => x.Name == "IPropBag" || x.Name == "IPropBagMin");
        }

        public override bool Equals(object obj)
        {
            var oLD = obj as PropBagMapperKey_OLD<TSource, TDestination>;
            return oLD != null &&
                   EqualityComparer<IMapTypeDefinition<TSource>>.Default.Equals(SourceTypeDef, oLD.SourceTypeDef) &&
                   EqualityComparer<IMapTypeDefinition<TDestination>>.Default.Equals(DestinationTypeDef, oLD.DestinationTypeDef) &&
                   MappingStrategy == oLD.MappingStrategy;
        }

        public override int GetHashCode()
        {
            var hashCode = 1208457409;
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition<TSource>>.Default.GetHashCode(SourceTypeDef);
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition<TDestination>>.Default.GetHashCode(DestinationTypeDef);
            hashCode = hashCode * -1521134295 + MappingStrategy.GetHashCode();
            return hashCode;
        }

        public bool Equals(PropBagMapperKey_OLD<TSource, TDestination> other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IPropBagMapperKey<TSource, TDestination> other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(PropBagMapperKeyGen other)
        {
            return other != null &&
                   MappingStrategy == other.MappingStrategy &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);
        }

        public bool Equals(IPropBagMapperKeyGen other)
        {
            return other != null &&
                   MappingStrategy == other.MappingStrategy &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);
        }

        public static bool operator ==(PropBagMapperKey_OLD<TSource, TDestination> oLD1, PropBagMapperKey_OLD<TSource, TDestination> oLD2)
        {
            return EqualityComparer<PropBagMapperKey_OLD<TSource, TDestination>>.Default.Equals(oLD1, oLD2);
        }

        public static bool operator !=(PropBagMapperKey_OLD<TSource, TDestination> oLD1, PropBagMapperKey_OLD<TSource, TDestination> oLD2)
        {
            return !(oLD1 == oLD2);
        }
    }

}
