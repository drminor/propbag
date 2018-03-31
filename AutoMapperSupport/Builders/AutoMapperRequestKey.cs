using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    // TODO: check to see if we really need to use the base class: PropBagMapperKeyGen

    public class AutoMapperRequestKey<TSource, TDestination>
            :   AutoMapperRequestKeyGen,
                IAutoMapperRequestKey<TSource, TDestination>, 
                IEquatable<IAutoMapperRequestKey<TSource, TDestination>>,
                IEquatable<AutoMapperRequestKey<TSource, TDestination>>
        //where TDestination: class, IPropBag
    {

        #region Public Properties

        public IMapTypeDefinition SourceTypeDef { get; }
        public IMapTypeDefinition DestinationTypeDef { get; }

        //public Func<TDestination, TSource> SourceConstructor { get; }
        //public Func<TSource, TDestination> DestinationConstructor { get; }

        public IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }

        #endregion

        #region Constructor

        public AutoMapperRequestKey
            (
            IBuildAutoMapper<TSource, TDestination> autoMapperBuilder,
            IConfigureAMapper<TSource, TDestination> mappingConfiguration,
            IMapTypeDefinition sourceMapTypeDef,
            IMapTypeDefinition destinationMapTypeDef
            )
            : base
            (
                autoMapperBuilder.GenRawAutoMapperCreator,
                sourceMapTypeDef,
                destinationMapTypeDef
            )
        {
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

        private bool Validate(IAutoMapperRequestKey<TSource, TDestination> mapperRequestKey)
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
            return Equals(obj as AutoMapperRequestKeyGen);
        }

        [System.Diagnostics.DebuggerStepThrough()]
        public override int GetHashCode()
        {
            var hashCode = 1780333077;
            //hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition>.Default.GetHashCode(SourceTypeDef);
            hashCode = hashCode * -1521134295 + EqualityComparer<IMapTypeDefinition>.Default.GetHashCode(DestinationTypeDef);
            return hashCode;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public bool Equals(IAutoMapperRequestKey<TSource, TDestination> other)
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

        public bool Equals(AutoMapperRequestKey<TSource, TDestination> other)
        {
            return ((IAutoMapperRequestKey<TSource, TDestination>)this).Equals(other);
        }

        // TODO: Can we do better than using the Default EqualityComparer?
        public static bool operator ==(AutoMapperRequestKey<TSource, TDestination> key1, AutoMapperRequestKey<TSource, TDestination> key2)
        {
            //return EqualityComparer<PropBagMapperKey<TSource, TDestination>>.Default.Equals(key1, key2);
            return ((IAutoMapperRequestKey<TSource, TDestination>)key1).Equals(key2);
        }

        public static bool operator !=(AutoMapperRequestKey<TSource, TDestination> key1, AutoMapperRequestKey<TSource, TDestination> key2)
        {
            return !(key1 == key2);
        }
        #endregion
    }
}
