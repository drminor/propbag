using System;

namespace Swhp.AutoMapperSupport
{
    public interface IMapTypeDefinition : IEquatable<IMapTypeDefinition>
    {
        /// <summary>
        /// The type of object that will serve as the source or destination in
        /// Mapping operations. This is the type parameter in the Generic IMapTypeDefinition[T]
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// If NewEmittedType is non-null, the value of NewEmittedType, otherwise the value of TargetType. 
        /// </summary>
        Type RunTimeType { get; }

        /// <summary>
        /// True if this for a type that implements the IPropBag interface.
        /// </summary>
        bool IsPropBag { get; }

        /// If set, an object reference used to distingish between mappers that have an identical RunTimeType.
        /// This value is not used in any way other than to compare to IMapTypeDefinition instances.
        object UniqueRef { get; }

        /// <summary>
        /// If set, a string used to distingish between mappers that have an identical RunTimeType.
        /// This value is not used in any way other than to compare to IMapTypeDefinition instances.
        /// </summary>
        string UniqueToken { get; }

        ///// <summary>
        ///// When a Proxy or Wrapper Type must be created to support the mapping operation, this holds the
        ///// new emitted type.
        ///// </summary>
        //Type NewEmittedType { get; }
    }
}
