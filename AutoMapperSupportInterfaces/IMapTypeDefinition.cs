using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

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
        string FullClassName { get; }

        /// <summary>
        /// The type of object that will serve as the source or destination in
        /// Mapping operations. This is the type parameter in the Generic IMapTypeDefinition[T]
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// True if Type (the above Type property) implements the IPropBag interface.
        /// </summary>
        bool IsPropBag { get; }

        /// <summary>
        /// The PropModel to use when constructing new instances of objects of Type: Type or of Type: NewWrapperType.
        /// </summary>
        PropModelType PropModel { get; }

        /// <summary>
        /// The PropFactory to use when constructing new instances.
        /// If set, it overrides the PropFactory setting on the PropModel.
        /// </summary>
        IPropFactory PropFactory { get; }

        /// <summary>
        /// The Type for which the proxy will stand in, or the Wrapper will wrap.
        /// This type must implement IPropBag and is usually PropBag or the same as the target type above.
        /// </summary>
        Type TypeToWrap { get; }

        /// <summary>
        /// When a Proxy or Wrapper Type must be created to support the mapping operation, this holds the
        /// new emitted type.
        /// </summary>
        Type NewWrapperType { get; set; }
    }
}
