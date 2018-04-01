using System;
using System.Collections.Generic;

namespace Swhp.AutoMapperSupport
{
    //using PropModelType = IPropModel<String>;

    public class MapTypeDefinition : IMapTypeDefinition, IEquatable<IMapTypeDefinition>, IEquatable<MapTypeDefinition>
    {
        #region Public Properties

        public Type TargetType { get; }
        public Type NewEmittedType { get; }
        public Type RunTimeType { get; }

        public object UniqueRef { get; }
        public string UniqueToken { get; }

        public bool IsPropBag { get; }

        public object PropModel { get; }

        //public Type TypeToWrap => TargetType; // PropModel?.TypeToWrap;
        // If specified, use the PropFactory provided by the caller instead of the one specified by the PropModel.
        //public object PropFactory => _propFactory /*?? PropModel?.PropFactory*/;

        #endregion

        #region Constructors

        // For non-IPropBag based types.
        public MapTypeDefinition(Type targetType) : this(targetType, null, null)
        {
        }

        // For non-IPropBag based types.
        public MapTypeDefinition(Type targetType, object uniqueRef) : this(targetType, uniqueRef, null)
        {
        }

        public MapTypeDefinition(Type targetType, object uniqueRef, string uniqueToken)
        {
            TargetType = targetType;
            NewEmittedType = null;
            RunTimeType = TargetType;

            //UniqueRef = uniqueRef ?? throw new ArgumentNullException(nameof(uniqueRef));
            //UniqueToken = uniqueToken;

            UniqueRef = uniqueRef ?? throw new ArgumentNullException("Must have a propModel for now.");
            UniqueToken = null; // Set this to null in all cases, for now.

            IsPropBag = true; // TODO: Get this from our caller.

            _hashCode = ComputeHashCode();
        }

        //private void CheckRunTimeType(PropModelType propModel)
        //{
        //    if (propModel != null)
        //    {
        //        if (propModel.RunTimeType == null)
        //        {
        //            throw new InvalidOperationException("The PropModel's RunTimeType is null.");
        //        }

        //        if (propModel.RunTimeType != typeof(T))
        //        {
        //            //System.Diagnostics.Debug.WriteLine($"Warning: The type parameter T: ({typeof(T)}) does not equal the " +
        //            //    $"PropModel's RunTimeType: ({propModel.RunTimeType}).");

        //            throw new InvalidOperationException($"The type parameter T: ({typeof(T)}) does not equal the " +
        //                $"PropModel's RunTimeType: ({propModel.RunTimeType}).");
        //        }
        //    }
        //}

        #endregion

        #region IEquatable Support and Object Overrides

        public override string ToString()
        {
            if(IsPropBag)
            {
                //return $"MapTypeDef for IPropBag type: {TargetType.ToString()}, with {PropModel.Count} properties.";
                return $"MapTypeDef for IPropBag type: {TargetType.ToString()}.";
            }
            else
            {
                return $"MapTypeDef for 'regular' type: {TargetType.ToString()}";
            }
        }

        // In order for the two MapTypeDefinitions to be equal they must reference the same instance of the IPropModel<LT2Raw>.
        // Clients of this service cache the PropModels so it should be easy to supply these unique references.
        public override bool Equals(object obj)
        {
            return obj != null
                && ((IMapTypeDefinition)this).Equals(obj as IMapTypeDefinition);
        }

        bool IEquatable<MapTypeDefinition>.Equals(MapTypeDefinition other)
        {
            return other != null
                && ((IMapTypeDefinition)this).Equals(other);
        }

        bool IEquatable<IMapTypeDefinition>.Equals(IMapTypeDefinition other)
        {
            return other != null
                && EqualityComparer<Type>.Default.Equals(TargetType, other.TargetType)
                && ReferenceEquals(UniqueRef, other.UniqueRef)
                && EqualityComparer<String>.Default.Equals(UniqueToken, other.UniqueToken);
        }

        private int _hashCode;
        public override int GetHashCode()
        {
            return _hashCode;
        }

        private int ComputeHashCode()
        {
            var hashCode = -2069486862;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TargetType);
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(UniqueRef);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UniqueToken);
            return hashCode;
        }
 
        public static bool operator ==(MapTypeDefinition definition1, MapTypeDefinition definition2)
        {
            return EqualityComparer<MapTypeDefinition>.Default.Equals(definition1, definition2);
        }

        public static bool operator !=(MapTypeDefinition definition1, MapTypeDefinition definition2)
        {
            return !(definition1 == definition2);
        }

        #endregion IEquatable Support and Object Overrides
    }
}
