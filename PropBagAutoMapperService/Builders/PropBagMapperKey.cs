using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public class PropBagMapperKey<TSource, TDestination>
            :   PropBagMapperKeyGen,
                IPropBagMapperKey<TSource, TDestination>,
                IEquatable<IPropBagMapperKey<TSource, TDestination>>,
                IEquatable<PropBagMapperKey<TSource, TDestination>>
        where TDestination: class, IPropBag
    {
        #region Private Members

        private readonly IBuildPropBagMapper<TSource, TDestination> _propBagMapperBuilder;

        #endregion

        #region Public Properties

        public IAutoMapperRequestKey<TSource, TDestination> AutoMapperRequestKey => (IAutoMapperRequestKey<TSource, TDestination>) AutoMapperRequestKeyGen;

        public IMapTypeDefinition<TSource> SourceTypeDef => AutoMapperRequestKey.SourceTypeDef;
        public IMapTypeDefinition<TDestination> DestinationTypeDef => AutoMapperRequestKey.DestinationTypeDef;

        public IConfigureAMapper<TSource, TDestination> MappingConfiguration => AutoMapperRequestKey.MappingConfiguration;

        public IPropBagMapper<TSource, TDestination> GeneratePropBagMapper
            (
            IPropBagMapperKey<TSource, TDestination> mapperRequestKey,
            ViewModelFactoryInterface viewModelFactory
            )
        {
            var result = _propBagMapperBuilder.GeneratePropBagMapper(mapperRequestKey, viewModelFactory);
            return result;
        }


        #endregion

        #region Constructor

        public PropBagMapperKey
        (
            IBuildPropBagMapper<TSource, TDestination> propBagMapperBuilder,
            IAutoMapperRequestKey<TSource, TDestination> autoMapperRequestKey
        )
        : base
        (
            propBagMapperBuilder.GenPropBagMapperCreator,
            autoMapperRequestKey
        )
        {
            _propBagMapperBuilder = propBagMapperBuilder;

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

        [System.Diagnostics.DebuggerStepThrough()]
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
            return ((IPropBagMapperKey<TSource, TDestination>)this).Equals(other);
        }

        // TODO: Can we do better than using the Default EqualityComparer?
        public static bool operator ==(PropBagMapperKey<TSource, TDestination> key1, PropBagMapperKey<TSource, TDestination> key2)
        {
            //return EqualityComparer<PropBagMapperKey<TSource, TDestination>>.Default.Equals(key1, key2);
            return ((IPropBagMapperKey<TSource, TDestination>)key1).Equals(key2);
        }

        public static bool operator !=(PropBagMapperKey<TSource, TDestination> key1, PropBagMapperKey<TSource, TDestination> key2)
        {
            return !(key1 == key2);
        }
        #endregion
    }
}
