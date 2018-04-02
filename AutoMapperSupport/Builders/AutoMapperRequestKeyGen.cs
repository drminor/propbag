using AutoMapper;
using System;

namespace Swhp.AutoMapperSupport
{
    // TODO: Consider implementing IDisposable -- to let go of any references we may hold to the mapper creators.
    public class AutoMapperRequestKeyGen : IAutoMapperRequestKeyGen, IEquatable<IAutoMapperRequestKeyGen>, IEquatable<AutoMapperRequestKeyGen>
    {
        #region Public Properties

        public IMapTypeDefinition SourceTypeDef { get; }
        public IMapTypeDefinition DestinationTypeDef { get; }

        public IAutoMapperConfigDetails AutoMapperConfigDetails { get; }

        public Func<IAutoMapperRequestKeyGen, IMapper> AutoMapperBuilder { get; }

        //public IMapper AutoMapper { get; set; }

        public Type SourceType => SourceTypeDef.TargetType;
        public Type DestinationType => DestinationTypeDef.TargetType;

        #endregion

        #region Constructor

        public AutoMapperRequestKeyGen
            (
            IMapTypeDefinition sourceTypeGenDef,
            IMapTypeDefinition destinationTypeGenDef,
            IAutoMapperConfigDetails autoMapperConfigDetails,
            Func<IAutoMapperRequestKeyGen, IMapper> autoMapperBuilder)
        {
            SourceTypeDef = sourceTypeGenDef;
            DestinationTypeDef = destinationTypeGenDef;
            AutoMapperConfigDetails = autoMapperConfigDetails;
            AutoMapperBuilder = autoMapperBuilder;
        }

        #endregion

        #region Public Methods

        public IMapper BuildAutoMapper()
        {
            return AutoMapperBuilder(this);
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
            return $"PropBagMapperKey: S={SourceTypeDef.ToString()}, D={DestinationTypeDef.ToString()}";
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
