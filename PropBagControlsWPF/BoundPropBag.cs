using System;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System.Reflection;
using System.Collections.Generic;

namespace DRM.PropBag.ControlsWPF
{
    // TODO: PropModel currently embodies a type definition with a set of inital values.
    // in order to treat the pairing of DerivedType + WrapperType as a unique identity,
    // the set of initial values needs to be pulled out of the PropModel.
    public struct BoundPropBag : IEquatable<BoundPropBag>
    {
        public BoundPropBag(PropModel propModel, Type dtViewModelType, Type rtViewModelType, PropertyInfo classAccessor)
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

        public override bool Equals(object other) => other is BoundPropBag && Equals((BoundPropBag)other);

        public bool Equals(BoundPropBag other) => DtViewModelType == other.DtViewModelType && PropModel == other.PropModel;

        public override int GetHashCode()
        {
            var hashCode = -1462822583;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(DtViewModelType);
            hashCode = hashCode * -1521134295 + PropModel.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(RtViewModelType);
            hashCode = hashCode * -1521134295 + EqualityComparer<PropertyInfo>.Default.GetHashCode(ClassAccessor);
            return hashCode;
        }

        public static bool operator ==(BoundPropBag left, BoundPropBag right) => left.Equals(right);

        public static bool operator !=(BoundPropBag left, BoundPropBag right) => !left.Equals(right);

    }
}
