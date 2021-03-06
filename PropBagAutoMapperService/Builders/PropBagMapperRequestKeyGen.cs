﻿using AutoMapper;
using Swhp.AutoMapperSupport;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    // TODO: Consider implementing IDisposable -- to let go of any references we may hold to the mapper creators.
    public class PropBagMapperRequestKeyGen : IPropBagMapperRequestKeyGen, IEquatable<IPropBagMapperRequestKeyGen>, IEquatable<PropBagMapperRequestKeyGen>
    {
        #region Private Properties

        internal IAutoMapperRequestKeyGen AutoMapperRequestKeyGen { get; }

        #endregion

        #region Public Properties

        public IMapper AutoMapper { get; internal set; }

        public Func<IPropBagMapperRequestKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> PropBagMapperCreator { get; }


        public Type SourceType => SourceTypeDef.TargetType;
        public Type DestinationType => DestinationTypeDef.TargetType;

        public IMapTypeDefinition SourceTypeDef => AutoMapperRequestKeyGen.SourceTypeDef;
        public IMapTypeDefinition DestinationTypeDef => AutoMapperRequestKeyGen.DestinationTypeDef;

        public IPropBagMapperConfigDetails PropBagMapperConfigDetails => AutoMapperRequestKeyGen.AutoMapperConfigDetails as IPropBagMapperConfigDetails;

        public PropModelType PropModel => PropBagMapperConfigDetails.PropModel;

        #endregion

        #region Constructor

        public PropBagMapperRequestKeyGen
            (
            Func<IPropBagMapperRequestKeyGen, ViewModelFactoryInterface, IPropBagMapperGen> mapperCreator,
            IAutoMapperRequestKeyGen autoMapperRequestKeyGen
            //,
            //PropModelType propModel
            )
        {
            PropBagMapperCreator = mapperCreator;
            AutoMapperRequestKeyGen = autoMapperRequestKeyGen;
            //PropModel = propModel;
        }

        #endregion

        #region Public Methods

        public IPropBagMapperGen CreatePropBagMapper(ViewModelFactoryInterface viewModelFactory)
        {
            return PropBagMapperCreator(this, viewModelFactory);
        }

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
               SourceTypeDef == other.SourceTypeDef &&
               DestinationTypeDef == other.DestinationTypeDef;
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
