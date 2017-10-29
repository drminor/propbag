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
    // TODO: check to see if we really needing to use the base class: PropBagMapperKeyGen

    public class PropBagMapperKey<TSource, TDestination> : PropBagMapperKeyGen,
        IPropBagMapperKey<TSource, TDestination>, 
        IEquatable<IPropBagMapperKey<TSource, TDestination>>,
        IEquatable<PropBagMapperKey<TSource, TDestination>> where TDestination: class, IPropBag
    {
        #region Private Members

        private IBuildPropBagMapper<TSource, TDestination> PropBagMapperBuilder { get; }

        #endregion

        #region Public Properties

        public IMapTypeDefinition<TSource> SourceTypeDef { get; }
        public IMapTypeDefinition<TDestination> DestinationTypeDef { get; }
        public Func<TDestination, TSource> SourceConstructor { get; }
        public Func<TSource, TDestination> DestinationConstructor { get; }

        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> MapperCreator => PropBagMapperBuilder.GenMapperCreator;

        #endregion

        #region Constructor

        //public PropBagMapperKey(PropModel pm, Type baseType,
        //    IBuildPropBagMapper<TSource, TDestination> propBagMapperBuilder,
        //    Func<TDestination, TSource> constructSourceFunc = null,
        //    Func<TSource, TDestination> constructDestinationFunc = null)
        //    : base(
        //          GetTypeDef<TSource>(pm, baseType) as IMapTypeDefinitionGen,
        //          GetTypeDef<TDestination>(pm, baseType) as IMapTypeDefinitionGen,
        //          propBagMapperBuilder.GenMapperCreator)
        //{
        //    Type tDest = typeof(TDestination);
        //    PropBagMapperBuilder = propBagMapperBuilder;

        //    if (typeof(TSource).IsPropBagBased()) throw new ApplicationException("The first type, TSource, is expected to be a regular, i.e., non-propbag-based type.");
        //    if (! (typeof(TDestination).IsPropBagBased())) throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");

        //    SourceTypeDef = base.SourceTypeGenDef as IMapTypeDefinition<TSource>; //GetTypeDef<TSource>(pm, baseType);
        //    DestinationTypeDef = base.DestinationTypeGenDef as IMapTypeDefinition<TDestination>; // GetTypeDef<TDestination>(pm, baseType);

        //    SourceConstructor = constructSourceFunc;
        //    DestinationConstructor = constructDestinationFunc;
        //}

        public PropBagMapperKey(
            IBuildPropBagMapper<TSource, TDestination> propBagMapperBuilder,
            IMapTypeDefinition<TSource> sourceMapTypeDef,
            IMapTypeDefinition<TDestination> destinationMapTypeDef,
            Func<TDestination, TSource> sourceConstructor = null,
            Func<TSource, TDestination> destinationConstructor = null)
            : base(propBagMapperBuilder.GenMapperCreator, sourceMapTypeDef, destinationMapTypeDef)
        {
            PropBagMapperBuilder = propBagMapperBuilder;

            if (sourceMapTypeDef.IsPropBag) throw new ApplicationException
                    ("The first type, TSource, is expected to be a regular, i.e., non-propbag-based type.");

            if (!destinationMapTypeDef.IsPropBag) throw new ApplicationException
                    ("The second type, TDestination, is expected to be a propbag-based type.");

            SourceTypeDef = sourceMapTypeDef;
            DestinationTypeDef = destinationMapTypeDef;

            SourceConstructor = sourceConstructor;
            DestinationConstructor = destinationConstructor;
        }

        #endregion

        //private static IMapTypeDefinition<T> GetTypeDef<T>(PropModel pm, Type baseType)
        //{
        //    if (typeof(T).IsPropBagBased())
        //    {
        //        return new MapTypeDefinition<T>(pm, baseType);
        //    }
        //    else
        //    {
        //        return new MapTypeDefinition<T>();
        //    }
        //}

        #region IEquatable Support and Object Overrides
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
        #endregion
    }



}
