using System;

namespace Swhp.AutoMapperSupport
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
        /// The type of object that will serve as the source or destination in
        /// Mapping operations. This is the type parameter in the Generic IMapTypeDefinition[T]
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// When a Proxy or Wrapper Type must be created to support the mapping operation, this holds the
        /// new emitted type.
        /// </summary>
        Type NewEmittedType { get; }

        /// <summary>
        /// If NewEmittedType is non-null, the value of NewEmittedType, otherwise the value of TargetType. 
        /// </summary>
        Type RunTimeType { get; }

        /// If set, an object reference used to distingish between mappers that have an identical RunTimeType.
        /// This value is not used in any way other than to compare to IMapTypeDefinition instances.
        object UniqueRef { get; }

        /// <summary>
        /// If set, a string used to distingish between mappers that have an identical RunTimeType.
        /// This value is not used in any way other than to compare to IMapTypeDefinition instances.
        /// </summary>
        string UniqueToken { get; }

        /// <summary>
        /// True if this for a type that implements the IPropBag interface.
        /// </summary>
        bool IsPropBag { get; }

        ///// <summary>
        ///// The Type for which the proxy will stand in, or the Wrapper will wrap.
        ///// This type must implement IPropBag and is usually PropBag or the same as the target type above.
        ///// </summary>
        //Type TypeToWrap { get; }


        ///// <summary>
        ///// If set, the PropFactory to use instead of the one recorded in the PropModel.
        ///// </summary>
        //object PropFactory { get; }

        /// The PropModel to use when constructing new instances of objects of Type: Type or of Type: NewWrapperType.
        //object PropModel { get; }

    }
}
