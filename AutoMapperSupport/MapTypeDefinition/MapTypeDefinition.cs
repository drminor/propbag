using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    //using PropModelType = IPropModel<String>;

    public class MapTypeDefinition : IMapTypeDefinition, IEquatable<IMapTypeDefinition>, IEquatable<MapTypeDefinition>
    {
        #region Private Properties

        object _propFactory { get; }
        string _fullClassName { get; }

        #endregion

        #region Public Properties

        public Type TargetType { get; }

        public bool IsPropBag { get; }
        public object PropModel { get; }

        public Type TypeToWrap => TargetType; // PropModel?.TypeToWrap;

        public Type NewEmittedType => TargetType; // PropModel?.NewEmittedType;

        public Type RunTimeType => /*PropModel?.RunTimeType ??*/ TargetType;

        // If specified, use the PropFactory provided by the caller instead of the one specified by the PropModel.
        public object PropFactory => _propFactory /*?? PropModel?.PropFactory*/;

        // If specified, use the FullClassName provided by the caller instead of the one specified by the PropModel.
        public string FullClassName => _fullClassName/* ?? PropModel?.FullClassName*/;

        #endregion

        #region Constructors

        // For non-IPropBag based types.
        public MapTypeDefinition(Type targetType)
        {
            TargetType = targetType;
            IsPropBag = false;
            PropModel = null;
            _fullClassName = null;
            _propFactory = null;

            _hashCode = ComputeHashCode();
        }

        // For non-IPropBag based types.
        public MapTypeDefinition(Type targetType, string fullClassName)
        {
            TargetType = targetType;
            IsPropBag = false;
            PropModel = null;
            _fullClassName = fullClassName ?? targetType.FullName;
            _propFactory = null;

            _hashCode = ComputeHashCode();
        }

        public MapTypeDefinition(object propModel, Type targetType, object propFactory, string fullClassName)
        {
            //CheckRunTimeType(propModel);

            TargetType = targetType;
            IsPropBag = true;
            PropModel = propModel ?? throw new ArgumentNullException(nameof(propModel));

            _fullClassName = fullClassName;
            _propFactory = propFactory;

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
                && ReferenceEquals(PropModel, other.PropModel)
                //&& EqualityComparer<Type>.Default.Equals(TypeToWrap, other.TypeToWrap)
                //&& EqualityComparer<IPropFactory>.Default.Equals(PropFactory, other.PropFactory)
                && ReferenceEquals(PropFactory, other.PropFactory)
                && EqualityComparer<String>.Default.Equals(FullClassName, other.FullClassName);
        }


        private int _hashCode;
        public override int GetHashCode()
        {
            return _hashCode;
        }

        private int ComputeHashCode()
        {
            try
            {
                var hashCode = 1000498031;
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TargetType);

                if(PropModel != null)
                {
                    hashCode = hashCode * -1521134295 + PropModel.GetHashCode();
                }

                if (PropFactory != null)
                {
                    hashCode = hashCode * -1521134295 + PropFactory.GetHashCode();
                }

                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullClassName);
                return hashCode;
            } 
            catch
            {
                throw;
            }
        }

        ///<remarks>
        /// We are not a IMapTypeDefinitionGen and so we cannot implement these operators for the
        /// IMapTypeDefinitionGen interface.
        ///</remarks>

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
