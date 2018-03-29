using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    // TODO: Consider implementing IDisposable -- to let go of any references we may hold to the mapper creators.
    public class PropBagMapperKeyGen : IPropBagMapperKeyGen, IEquatable<IPropBagMapperKeyGen>, IEquatable<PropBagMapperKeyGen>
    {
        #region Public Properties

        public Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> PropBagMapperCreator { get; }

        public Func<IPropBagMapperKeyGen, IMapper> RawAutoMapperCreator { get; }

        public IMapTypeDefinitionGen SourceTypeGenDef { get; }
        public IMapTypeDefinitionGen DestinationTypeGenDef { get; }

        public IMapper AutoMapper { get; set; }

        #endregion

        #region Constructor

        public PropBagMapperKeyGen
            (
            Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> mapperCreator,
            Func<IPropBagMapperKeyGen, IMapper> rawAutoMapperCreator,
            IMapTypeDefinitionGen sourceTypeGenDef,
            IMapTypeDefinitionGen destinationTypeGenDef
            )
        {
            PropBagMapperCreator = mapperCreator;
            RawAutoMapperCreator = rawAutoMapperCreator;
            SourceTypeGenDef = sourceTypeGenDef;
            DestinationTypeGenDef = destinationTypeGenDef;
        }

        #endregion

        #region Public Methods

        public IPropBagMapperGen CreatePropBagMapper(ViewModelFactoryInterface viewModelFactory)
        {
            return PropBagMapperCreator(this, viewModelFactory);
        }

        public IMapper CreateRawAutoMapper()
        {
            return RawAutoMapperCreator(this);
        }

        #endregion

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as IPropBagMapperKeyGen);
        }

        public bool Equals(PropBagMapperKeyGen other)
        {
            return Equals(other as IPropBagMapperKeyGen);
        }

        public bool Equals(IPropBagMapperKeyGen other)
        {
            //return other != null &&
            //       EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
            //       EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);

            return other != null &&
               SourceTypeGenDef == other.SourceTypeGenDef &&
               DestinationTypeGenDef == other.DestinationTypeGenDef;

        }

        public override int GetHashCode()
        {
            var hashCode = 1973524047;
            hashCode = hashCode * -1521134295 + SourceTypeGenDef.GetHashCode();
            hashCode = hashCode * -1521134295 + DestinationTypeGenDef.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"PropBagMapperKey: S={SourceTypeGenDef.ToString()}, D={DestinationTypeGenDef.ToString()}";
        }

        public static bool operator ==(PropBagMapperKeyGen gen1, PropBagMapperKeyGen gen2)
        {
            //return EqualityComparer<PropBagMapperKeyGen>.Default.Equals(gen1, gen2);
            return ((IPropBagMapperKeyGen)gen1).Equals(gen2);
        }

        public static bool operator !=(PropBagMapperKeyGen gen1, PropBagMapperKeyGen gen2)
        {
            return !(gen1 == gen2);
        }

        #endregion
    }
}
