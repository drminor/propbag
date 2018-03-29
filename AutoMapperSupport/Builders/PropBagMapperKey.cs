using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;


namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    // TODO: check to see if we really need to use the base class: PropBagMapperKeyGen

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
        //public Func<TDestination, TSource> SourceConstructor { get; }
        //public Func<TSource, TDestination> DestinationConstructor { get; }

        public Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> MapperCreator => PropBagMapperBuilder.GenMapperCreator;

        public IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }

        #endregion

        #region Constructor

        public PropBagMapperKey
            (
            IBuildPropBagMapper<TSource, TDestination> propBagMapperBuilder,
            //ViewModelFactoryInterface viewModelFactory,
            IConfigureAMapper<TSource, TDestination> mappingConfiguration,
            IMapTypeDefinition<TSource> sourceMapTypeDef,
            IMapTypeDefinition<TDestination> destinationMapTypeDef
            )
            : base
            (
                propBagMapperBuilder.GenMapperCreator,
                propBagMapperBuilder.RawAutoMapperCreator,
                sourceMapTypeDef,
                destinationMapTypeDef
            )
        {
            PropBagMapperBuilder = propBagMapperBuilder;
            MappingConfiguration = mappingConfiguration;

            SourceTypeDef = sourceMapTypeDef;
            DestinationTypeDef = destinationMapTypeDef;

            //SourceConstructor = mappingConfiguration.SourceConstructor;
            //DestinationConstructor = mappingConfiguration.DestinationConstructor;

            AutoMapper = null;

            ValidateThisKey();
        }

        #endregion

        [System.Diagnostics.Conditional("DEBUG")]
        private void ValidateThisKey()
        {
            Validate(this);
        }

        private bool Validate(IPropBagMapperKey<TSource, TDestination> mapperRequestKey)
        {
            if (mapperRequestKey.MappingConfiguration.RequiresWrappperTypeEmitServices)
            {
                if (mapperRequestKey.SourceTypeDef.IsPropBag)
                    throw new ApplicationException("The first type, TSource, is expected to be a regular, i.e., non-propbag-based type.");

                if (!mapperRequestKey.DestinationTypeDef.IsPropBag)
                    throw new ApplicationException("The second type, TDestination, is expected to be a propbag-based type.");
            }

            return true;
        }

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as PropBagMapperKeyGen);
        }

        public override int GetHashCode()
        {
            var hashCode = 1780333077;
            //hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition<TSource>>.Default.GetHashCode(SourceTypeDef);
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition<TDestination>>.Default.GetHashCode(DestinationTypeDef);
            return hashCode;
        }


        public override string ToString()
        {
            return base.ToString();
        }

        public bool Equals(IPropBagMapperKey<TSource, TDestination> other)
        {
            if (other == null) return false;
            if ( (this.DestinationTypeDef.Equals(other.DestinationTypeDef)) && this.SourceTypeDef.Equals(other.SourceTypeDef))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Equals(PropBagMapperKey<TSource, TDestination> other)
        {
            return ( ((IPropBagMapperKey<TSource, TDestination>)this).Equals(other) );
        }

        // TODO: Can we do better than using the Default EqualityComparer?
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
