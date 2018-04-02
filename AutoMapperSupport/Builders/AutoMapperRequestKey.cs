using System;
using System.Collections.Generic;

namespace Swhp.AutoMapperSupport
{
    public class AutoMapperRequestKey<TSource, TDestination>
            :   AutoMapperRequestKeyGen,
                IAutoMapperRequestKey<TSource, TDestination>, 
                IEquatable<IAutoMapperRequestKey<TSource, TDestination>>,
                IEquatable<AutoMapperRequestKey<TSource, TDestination>>
    {
        #region Public Properties

        public IConfigureAMapper<TSource, TDestination> MappingConfiguration { get; }

        //public Func<TDestination, TSource> SourceConstructor => MappingConfiguration.SourceConstructor;
        //public Func<TSource, TDestination> DestinationConstructor => MappingConfiguration.DestinationConstructor;

        #endregion

        #region Constructor

        public AutoMapperRequestKey
            (
            IMapTypeDefinition sourceMapTypeDef,
            IMapTypeDefinition destinationMapTypeDef,
            IAutoMapperConfigDetails autoMapperConfigDetails,
            IConfigureAMapper<TSource, TDestination> mappingConfiguration,
            IAutoMapperBuilder<TSource, TDestination> autoMapperBuilder
            )
            : base
            (
            sourceMapTypeDef,
            destinationMapTypeDef,
            autoMapperConfigDetails,
            autoMapperBuilder.AutoMapperBuilderGen
            )
        {
            MappingConfiguration = mappingConfiguration;
        }

        #endregion

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
