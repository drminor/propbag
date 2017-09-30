using System;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System.Reflection;

namespace DRM.PropBag.ControlsWPF
{
    // TODO: PropModel currently embodies a type definition with a set of inital values.
    // in order to treat the pairing of DerivedType + WrapperType as a unique identity,
    // the set of initial values needs to be pulled out of the PropModel.
    public struct BoundPropBag : IEquatable<BoundPropBag>
    {
        public BoundPropBag(Type dtViewModelType, PropModel propModel, Type rtViewModelType, PropertyInfo classAccessor)
        {
            DtViewModelType = dtViewModelType; 
            PropModel = propModel;
            RtViewModelType = rtViewModelType;
            ClassAccessor = classAccessor;
        }

        /// <summary>
        /// Type that is defined at compile time and is used in design mode.
        /// </summary>
        public Type DtViewModelType { get; }

        /// <summary>
        /// Information about the properties that the run-time type will provide.
        /// </summary>
        public PropModel PropModel { get; }

        /// <summary>
        /// Type that is created by Reflection.Emit to provide field, property, method and event accessors
        /// that can be used by the AutoMapper libary.
        /// These are discoverable by Reflection -- no intellisense.
        /// </summary>
        public Type RtViewModelType { get; }

        /// <summary>
        /// The property on the View to which this object can be accessed.
        /// </summary>
        public PropertyInfo ClassAccessor { get; }

        public override int GetHashCode()
        {
            return GenerateHash.CustomHash(DtViewModelType.GetHashCode(), PropModel.GetHashCode());
        }

        public override bool Equals(object other) => other is BoundPropBag && Equals((BoundPropBag)other);

        public bool Equals(BoundPropBag other) => DtViewModelType == other.DtViewModelType && PropModel == other.PropModel;

        public static bool operator ==(BoundPropBag left, BoundPropBag right) => left.Equals(right);

        public static bool operator !=(BoundPropBag left, BoundPropBag right) => !left.Equals(right);

    }
}
