﻿using AutoMapper;
using DRM.PropBag.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    // TODO: Consider implementing IDisposable -- to let go of any references we may hold to the mapper creators.
    public class PropBagMapperRequestKeyGen : IPropBagMapperRequestKeyGen, IEquatable<IPropBagMapperRequestKeyGen>, IEquatable<PropBagMapperRequestKeyGen>
    {
        #region Public Properties

        public Type SourceType => SourceTypeDef.TargetType;
        public Type DestinationType => DestinationTypeDef.TargetType;

        public IMapTypeDefinition SourceTypeDef => AutoMapperRequestKeyGen.SourceTypeGenDef;
        public IMapTypeDefinition DestinationTypeDef => AutoMapperRequestKeyGen.DestinationTypeGenDef;

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

        public Func<IPropBagMapperRequestKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> PropBagMapperCreator { get; }



        #endregion

        #region Constructor

        public PropBagMapperRequestKeyGen
            (
            Func<IPropBagMapperRequestKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> mapperCreator,
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
            return Equals(obj as IPropBagMapperRequestKeyGen);
        }

        public bool Equals(PropBagMapperRequestKeyGen other)
        {
            return Equals(other as IPropBagMapperRequestKeyGen);
        }

        public bool Equals(IPropBagMapperRequestKeyGen other)
        {
            //return other != null &&
            //       EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(SourceTypeGenDef, other.SourceTypeGenDef) &&
            //       EqualityComparer<IMapTypeDefinitionGen>.Default.Equals(DestinationTypeGenDef, other.DestinationTypeGenDef);

            return other != null &&
               AutoMapperRequestKeyGen.SourceTypeGenDef == other.SourceTypeDef &&
               AutoMapperRequestKeyGen.DestinationTypeGenDef == other.DestinationTypeDef;
        }

        public override int GetHashCode()
        {
            var hashCode = 1973524047;
            hashCode = hashCode * -1521134295 + SourceTypeDef.GetHashCode();
            hashCode = hashCode * -1521134295 + DestinationTypeDef.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"PropBagMapperKey: S={SourceTypeDef.ToString()}," +
                $" D={DestinationTypeDef.ToString()}";
        }

        public static bool operator ==(PropBagMapperRequestKeyGen gen1, PropBagMapperRequestKeyGen gen2)
        {
            //return EqualityComparer<PropBagMapperKeyGen>.Default.Equals(gen1, gen2);
            return ((IPropBagMapperRequestKeyGen)gen1).Equals(gen2);
        }

        public static bool operator !=(PropBagMapperRequestKeyGen gen1, PropBagMapperRequestKeyGen gen2)
        {
            return !(gen1 == gen2);
        }

        #endregion
    }
}
