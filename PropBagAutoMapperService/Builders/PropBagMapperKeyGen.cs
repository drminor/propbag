using AutoMapper;
using DRM.PropBag.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    // TODO: Consider implementing IDisposable -- to let go of any references we may hold to the mapper creators.
    public class PropBagMapperKeyGen : IPropBagMapperKeyGen, IEquatable<IPropBagMapperKeyGen>, IEquatable<PropBagMapperKeyGen>
    {
        #region Public Properties

        public Type SourceType => SourceTypeGenDef.TargetType;
        public Type DestinationType => DestinationTypeGenDef.TargetType;

        public IMapTypeDefinitionGen SourceTypeGenDef => AutoMapperRequestKeyGen.SourceTypeGenDef;
        public IMapTypeDefinitionGen DestinationTypeGenDef => AutoMapperRequestKeyGen.DestinationTypeGenDef;

        public IAutoMapperRequestKeyGen AutoMapperRequestKeyGen { get; }

        public IMapper AutoMapper
        {
            get
            {
                return AutoMapperRequestKeyGen.AutoMapper;
            }
            set
            {
                AutoMapperRequestKeyGen.AutoMapper = value;
            }
        }

        public Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> PropBagMapperCreator { get; }



        #endregion

        #region Constructor

        public PropBagMapperKeyGen
            (
            Func<IPropBagMapperKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> mapperCreator,
            IAutoMapperRequestKeyGen autoMapperRequestKeyGen
            )
        {
            PropBagMapperCreator = mapperCreator;
            AutoMapperRequestKeyGen = autoMapperRequestKeyGen;
        }

        #endregion

        #region Public Methods

        public IPropBagMapperGen CreatePropBagMapper(ViewModelFactoryInterface viewModelFactory)
        {
            return PropBagMapperCreator(this, viewModelFactory);
        }

        //public IMapper CreateRawAutoMapper()
        //{
        //    IAutoMapperRequestKeyGen amRequestKey = GetAutoMapperRequestKeyGen();
        //    return RawAutoMapperCreator(amRequestKey);
        //}

        //public IAutoMapperRequestKeyGen GetAutoMapperRequestKeyGen()
        //{
        //    IAutoMapperRequestKeyGen amRequestKey = new AutoMapperRequestKeyGen(RawAutoMapperCreator, SourceTypeGenDef, DestinationTypeGenDef);
        //    return amRequestKey;
        //}

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
               AutoMapperRequestKeyGen.SourceTypeGenDef == other.SourceTypeGenDef &&
               AutoMapperRequestKeyGen.DestinationTypeGenDef == other.DestinationTypeGenDef;
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
            return $"PropBagMapperKey: S={SourceTypeGenDef.ToString()}," +
                $" D={DestinationTypeGenDef.ToString()}";
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
