using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    /// <summary>
    /// This is the typed version of the IMapTypeDefinitionGen interface.
    /// It allows one to create a MapTypeDefinition using a type known at compile time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMapTypeDefinition<T> : IMapTypeDefinitionGen
    {
    }

    public interface IMapTypeDefinitionGen : IEquatable<IMapTypeDefinitionGen>
    {
        /// <summary>
        /// The type of object that will serve as the source or destination in
        /// Mapping operations.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// True if Type (the above Type property) implements the IPropBag interface.
        /// </summary>
        bool IsPropBag { get; }

        /// <summary>
        /// The PropModel to use when constructing new instances of objects of Type: Type or of Type: NewWrapperType.
        /// </summary>
        PropModel PropModel { get; }

        //// TODO: Do we need to keep this and keep the PropFactory property of PropModel.
        ///// <summary>
        ///// The PropFactory to use when constructing new instances.
        ///// </summary>
        //IPropFactory PropFactory { get; }

        /// <summary>
        /// When a Proxy or Wrapper Type must be created to support the mapping operation, this holds the
        /// new emitted type.
        /// </summary>
        Type NewWrapperType { get; }
    }
}
