using System;

namespace DRM.PropBag.AutoMapperSupport
{
    //using PropModelType = IPropModel<String>;

    ///// <summary>
    ///// This is the typed version of the IMapTypeDefinitionGen interface.
    ///// It allows one to create a MapTypeDefinition using a type known at compile time.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //public interface IMapTypeDefinition<T> : IMapTypeDefinitionGen
    //{
    //}

    public interface IMapTypeDefinition : IEquatable<IMapTypeDefinition>
    {
        /// <summary>
        /// The PropModel to use when constructing new instances of objects of Type: Type or of Type: NewWrapperType.
        /// </summary>
        object PropModel { get; }

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
        /// The Type for which the proxy will stand in, or the Wrapper will wrap.
        /// This type must implement IPropBag and is usually PropBag or the same as the target type above.
        /// </summary>
        Type TypeToWrap { get; }

        /// <summary>
        /// When a Proxy or Wrapper Type must be created to support the mapping operation, this holds the
        /// new emitted type.
        /// </summary>
        Type NewEmittedType { get; /*set;*/ }

        /// <summary>
        /// If NewEmittedType is non-null, the value of NewEmittedType, otherwise the value of TargetType. 
        /// </summary>
        Type RunTimeType { get; }

        /// <summary>
        /// If set, the PropFactory to use instead of the one recorded in the PropModel.
        /// </summary>
        object PropFactory { get; }

        /// <summary>
        /// If set, the FullClass name to use instead of the one recorded in the PropModel.
        /// </summary>
        string FullClassName { get; }
    }
}
