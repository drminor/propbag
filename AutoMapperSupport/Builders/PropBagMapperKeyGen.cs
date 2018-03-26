using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public class PropBagMapperKeyGen : IPropBagMapperKeyGen, IEquatable<IPropBagMapperKeyGen>, IEquatable<PropBagMapperKeyGen>
    {
        #region Private Members

        //private Func<IPropBagMapperKeyGen, IPropBagMapperGen> MapperCreator { get; }
        private Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> _mapperCreator { get; }
        private ViewModelFactoryInterface _viewModelFactory { get; }

        #endregion

        #region Public Properties

        public IMapTypeDefinitionGen SourceTypeGenDef { get; set; }
        public IMapTypeDefinitionGen DestinationTypeGenDef { get; set; }

        #endregion

        #region Constructor

        public PropBagMapperKeyGen
            (
            Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> mapperCreator,
            ViewModelFactoryInterface viewModelFactory,
            //Func<IPropBagMapperKeyGen, IPropBagMapperGen> mapperCreator,
            IMapTypeDefinitionGen sourceTypeGenDef,
            IMapTypeDefinitionGen destinationTypeGenDef
            )
        {
            _mapperCreator = mapperCreator;
            _viewModelFactory = viewModelFactory;
            SourceTypeGenDef = sourceTypeGenDef;
            DestinationTypeGenDef = destinationTypeGenDef;
        }

        #endregion

        #region Public Methods

        public IPropBagMapperGen CreateMapper()
        {
            return _mapperCreator(this, _viewModelFactory);
        }

        #endregion

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as PropBagMapperKeyGen);
        }

        public bool Equals(PropBagMapperKeyGen other)
        {
            return other != null &&
                   //MappingStrategy == other.MappingStrategy &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);
        }

        public bool Equals(IPropBagMapperKeyGen other)
        {
            return other != null &&
                   //MappingStrategy == other.MappingStrategy &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
                   EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);
        }

        public override int GetHashCode()
        {
            var hashCode = 1973524047;
            //hashCode = hashCode * -1521134295 + MappingStrategy.GetHashCode();
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
        #endregion
    }

    // Old version, works just as well without explicitly inheriting from class that implements IPropBagMapperKeyGen
    //class PropBagMapperKey_OLD<TSource, TDestination> : IPropBagMapperKey<TSource, TDestination>,
    //    IEquatable<PropBagMapperKey_OLD<TSource, TDestination>>, IEquatable<IPropBagMapperKey<TSource, TDestination>>,
    //    IEquatable<IPropBagMapperKeyGen>
    //{
    //    public Func<IPropBagMapperKeyGen, IPropBagMapperGen> CreateMapper { get; }

    //    public IMapTypeDefinition<TSource> SourceTypeDef { get; }

    //    public IMapTypeDefinition<TDestination> DestinationTypeDef { get; }

    //    public Func<TDestination, TSource> ConstructSourceFunc { get; }

    //    public Func<TSource, TDestination> ConstructDestinationFunc { get; }

    //    public PropBagMappingStrategyEnum MappingStrategy { get; set; }

    //    public IMapTypeDefinitionGen SourceTypeGenDef => SourceTypeDef as IMapTypeDefinitionGen;

    //    public IMapTypeDefinitionGen DestinationTypeGenDef => DestinationTypeDef as IMapTypeDefinitionGen;
    //    public PropBagMapperKey_OLD(PropModel pm,
    //        Type baseType,
    //        PropBagMappingStrategyEnum mappingStrategy,
    //        Func<TDestination, TSource> constructSourceFunc = null,
    //        Func<TSource, TDestination> constructDestinationFunc = null)
    //    {
    //        if (typeof(TSource) is IPropBag) throw new ApplicationException("The first type, TSource, is expected to be a regular, non-propbag-based type.");
    //        if (typeof(TDestination) is IPropBag) throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");

    //        SourceTypeDef = GetTypeDef<TSource>(pm, baseType);
    //        DestinationTypeDef = GetTypeDef<TDestination>(pm, baseType);

    //        ConstructSourceFunc = constructSourceFunc;
    //        ConstructDestinationFunc = constructDestinationFunc;

    //        MappingStrategy = mappingStrategy;

    //        CreateMapper = GetCreaterFunc(mappingStrategy);
    //    }

    //    private static Func<IPropBagMapperKeyGen, IPropBagMapperGen> GetCreaterFunc(PropBagMappingStrategyEnum mappingStrategy)
    //    {
    //        Func<IPropBagMapperKeyGen, IPropBagMapperGen> result;
    //        switch (mappingStrategy)
    //        {
    //            case PropBagMappingStrategyEnum.ExtraMembers:
    //                {
    //                    result = (untypedKey) =>
    //                    {
    //                        IPropBagMapperKey<TSource, TDestination> typedKey = (IPropBagMapperKey<TSource, TDestination>)untypedKey;
    //                        return new PropBagMapperCustom<TSource, TDestination>(typedKey);
    //                    };
    //                    break;
    //                }
    //            case PropBagMappingStrategyEnum.EmitProxy:
    //                {
    //                    result = (untypedKey) =>
    //                    {
    //                        IPropBagMapperKey<TSource, TDestination> typedKey = (IPropBagMapperKey<TSource, TDestination>)untypedKey;
    //                        return new SimplePropBagMapper<TSource, TDestination>(typedKey);
    //                    };
    //                    break;
    //                }
    //            default:
    //                {
    //                    throw new ApplicationException($"Unsupported, or unexpected value of {nameof(PropBagMappingStrategyEnum)}.");
    //                }
    //        }
    //        return result;
    //    }

    //    private static IMapTypeDefinition<T> GetTypeDef<T>(PropModel pm, Type baseType)
    //    {
    //        if (IsPropGenBased(typeof(T)))
    //        {
    //            return new MapTypeDefinition<T>(pm, baseType);
    //        }
    //        else
    //        {
    //            return new MapTypeDefinition<T>();
    //        }
    //    }

    //    private static bool IsPropGenBased(Type t)
    //    {
    //        //IEnumerable<Type> r = t.GetInterfaces();
    //        //Type a = t.GetInterfaces().FirstOrDefault(x => x.Name == "IPropBag");

    //        // TODO: Consider using ITypeSafePropBag instead of IPropBag
    //        return null != t.GetInterfaces().FirstOrDefault(x => x.Name == "IPropBag");
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        var oLD = obj as PropBagMapperKey_OLD<TSource, TDestination>;
    //        return oLD != null &&
    //               EqualityComparer<IMapTypeDefinition<TSource>>.Default.Equals(SourceTypeDef, oLD.SourceTypeDef) &&
    //               EqualityComparer<IMapTypeDefinition<TDestination>>.Default.Equals(DestinationTypeDef, oLD.DestinationTypeDef) &&
    //               MappingStrategy == oLD.MappingStrategy;
    //    }

    //    public override int GetHashCode()
    //    {
    //        var hashCode = 1208457409;
    //        hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition<TSource>>.Default.GetHashCode(SourceTypeDef);
    //        hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition<TDestination>>.Default.GetHashCode(DestinationTypeDef);
    //        hashCode = hashCode * -1521134295 + MappingStrategy.GetHashCode();
    //        return hashCode;
    //    }

    //    public bool Equals(PropBagMapperKey_OLD<TSource, TDestination> other)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool Equals(IPropBagMapperKey<TSource, TDestination> other)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool Equals(PropBagMapperKeyGen other)
    //    {
    //        return other != null &&
    //               MappingStrategy == other.MappingStrategy &&
    //               EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
    //               EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);
    //    }

    //    public bool Equals(IPropBagMapperKeyGen other)
    //    {
    //        return other != null &&
    //               MappingStrategy == other.MappingStrategy &&
    //               EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
    //               EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);
    //    }

    //    public static bool operator ==(PropBagMapperKey_OLD<TSource, TDestination> oLD1, PropBagMapperKey_OLD<TSource, TDestination> oLD2)
    //    {
    //        return EqualityComparer<PropBagMapperKey_OLD<TSource, TDestination>>.Default.Equals(oLD1, oLD2);
    //    }

    //    public static bool operator !=(PropBagMapperKey_OLD<TSource, TDestination> oLD1, PropBagMapperKey_OLD<TSource, TDestination> oLD2)
    //    {
    //        return !(oLD1 == oLD2);
    //    }
    //}
}
