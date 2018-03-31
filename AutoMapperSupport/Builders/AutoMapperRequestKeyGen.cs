using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    // TODO: Consider implementing IDisposable -- to let go of any references we may hold to the mapper creators.
    public class AutoMapperRequestKeyGen : IAutoMapperRequestKeyGen, IEquatable<IAutoMapperRequestKeyGen>, IEquatable<AutoMapperRequestKeyGen>
    {
        #region Public Properties

        public Type SourceType => SourceTypeGenDef.TargetType;
        public Type DestinationType => DestinationTypeGenDef.TargetType;

        public IMapTypeDefinition SourceTypeGenDef { get; }
        public IMapTypeDefinition DestinationTypeGenDef { get; }

        public Func<IAutoMapperRequestKeyGen, IMapper> RawAutoMapperCreator { get; }

        public IMapper AutoMapper { get; set; }

        #endregion

        #region Constructor

        public AutoMapperRequestKeyGen
            (
            Func<IAutoMapperRequestKeyGen, IMapper> rawAutoMapperCreator,
            IMapTypeDefinition sourceTypeGenDef,
            IMapTypeDefinition destinationTypeGenDef
            )
        {
            RawAutoMapperCreator = rawAutoMapperCreator;
            SourceTypeGenDef = sourceTypeGenDef;
            DestinationTypeGenDef = destinationTypeGenDef;
        }

        #endregion

        #region Public Methods

        public IMapper CreateRawAutoMapper()
        {
            return RawAutoMapperCreator(this);
        }

        #endregion

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as IAutoMapperRequestKeyGen);
        }

        public bool Equals(AutoMapperRequestKeyGen other)
        {
            return Equals(other as IAutoMapperRequestKeyGen);
        }

        public bool Equals(IAutoMapperRequestKeyGen other)
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

        public static bool operator ==(AutoMapperRequestKeyGen gen1, AutoMapperRequestKeyGen gen2)
        {
            //return EqualityComparer<PropBagMapperKeyGen>.Default.Equals(gen1, gen2);
            return ((IAutoMapperRequestKeyGen)gen1).Equals(gen2);
        }

        public static bool operator !=(AutoMapperRequestKeyGen gen1, AutoMapperRequestKeyGen gen2)
        {
            return !(gen1 == gen2);
        }

        #endregion
    }
}
