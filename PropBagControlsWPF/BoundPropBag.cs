using System;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.ControlsWPF
{
    // TODO: PropModel currently embodies a type definition with a set of inital values.
    // in order to treat the pairing of DerivedType + WrapperType as a unique identity,
    // the set of initial values needs to be pulled out of the PropModel.
    public struct BoundPropBag : IEquatable<BoundPropBag>
    {
        public BoundPropBag(Type derivingtype, PropModel propModel)
        {
            DerivingType = derivingtype;
            PropModel = propModel;

            WrapperType = null;
        }

        public Type DerivingType { get; }
        public PropModel PropModel { get; }
        public Type WrapperType { get; set; }

        public override int GetHashCode()
        {
            return GenerateHash.CustomHash(DerivingType.GetHashCode(), PropModel.GetHashCode());
        }

        public override bool Equals(object other) => other is BoundPropBag && Equals((BoundPropBag)other);

        public bool Equals(BoundPropBag other) => DerivingType == other.DerivingType && PropModel == other.PropModel;

        public static bool operator ==(BoundPropBag left, BoundPropBag right) => left.Equals(right);

        public static bool operator !=(BoundPropBag left, BoundPropBag right) => !left.Equals(right);

    }
}
