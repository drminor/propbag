using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropModelType = IPropModel<String>;

    public class MapTypeDefinition<T> : IMapTypeDefinition<T>, IEquatable<IMapTypeDefinitionGen>, IEquatable<MapTypeDefinition<T>>
    {
        #region Private Properties

        IPropFactory _propFactory { get; }
        string _fullClassName { get; }

        #endregion

        #region Public Properties

        public Type TargetType { get; }

        public bool IsPropBag { get; }
        public PropModelType PropModel { get; }

        public Type TypeToWrap => PropModel?.TypeToWrap;

        public Type NewEmittedType => PropModel?.NewEmittedType;

        public Type RunTimeType => PropModel?.RunTimeType ?? TargetType;

        // If specified, use the PropFactory provided by the caller instead of the one specified by the PropModel.
        public IPropFactory PropFactory => _propFactory ?? PropModel?.PropFactory;

        // If specified, use the FullClassName provided by the caller instead of the one specified by the PropModel.
        public string FullClassName => _fullClassName ?? PropModel?.FullClassName;

        #endregion

        #region Constructors

        // For non-IPropBag based types.
        public MapTypeDefinition(string fullClassName)
        {
            TargetType = typeof(T);
            IsPropBag = false;
            PropModel = null;
            //TypeToWrap = null;
            //NewEmittedType = null;
            _fullClassName = fullClassName;
            _propFactory = null;

            _hashCode = ComputeHashCode();
        }

        public MapTypeDefinition(PropModelType propModel/*, Type typeToWrap*/, string fullClassName, IPropFactory propFactory)
        {

            //if (propModel.DeriveFromClassMode == DeriveFromClassModeEnum.Custom && typeToWrap != null && propModel.TypeToWrap != typeToWrap)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Warning: the value for the PropModel's TypeToCreate property is not equal to the supplied typeToWrap.");
            //}

            //if (typeToWrap != null && typeToWrap != propModel.TypeToWrap)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Warning: the value for the PropModel's TypeToCreate property is not equal to the supplied typeToWrap.");
            //}

            TargetType = typeof(T);

            if(propModel != null)
            {
                if(propModel.RunTimeType == null)
                {
                    throw new InvalidOperationException("The PropModel's RunTimeType is null.");
                }

                if (propModel.RunTimeType != TargetType)
                {
                    //System.Diagnostics.Debug.WriteLine($"Warning: The type parameter T: ({typeof(T)}) does not equal the " +
                    //    $"PropModel's RunTimeType: ({propModel.RunTimeType}).");

                    throw new InvalidOperationException($"The type parameter T: ({typeof(T)}) does not equal the " +
                        $"PropModel's RunTimeType: ({propModel.RunTimeType}).");
                }
            }



            IsPropBag = true;
            PropModel = propModel ?? throw new ArgumentNullException(nameof(propModel));

            //TypeToWrap = typeToWrap ?? propModel.TypeToWrap; // throw new ArgumentNullException(nameof(typeToWrap));
            //NewEmittedType = propModel.NewEmittedType;

            _fullClassName = fullClassName;
            _propFactory = propFactory;
            _hashCode = ComputeHashCode();
        }

        #endregion

        // TODO: Check the IEquatable support for MapTypeDefinition.
        #region IEquatable Support and Object Overrides

        public override string ToString()
        {
            if(IsPropBag)
            {
                return $"MapTypeDef for IPropBag type: {TargetType.ToString()}, with {PropModel.Count} properties.";
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
                && ((IMapTypeDefinitionGen)this).Equals(obj as IMapTypeDefinitionGen);
        }

        #region IEquatable<MapTypeDefinition<T>>

        bool IEquatable<MapTypeDefinition<T>>.Equals(MapTypeDefinition<T> other)
        {
            return other != null
                && ((IMapTypeDefinitionGen)this).Equals(other);
        }

        #endregion

        #region IEquatable<IMapTypeDefinitionGen>

        bool IEquatable<IMapTypeDefinitionGen>.Equals(IMapTypeDefinitionGen other)
        {
            return other != null
                && EqualityComparer<Type>.Default.Equals(TargetType, other.TargetType)
                && ReferenceEquals(PropModel, other.PropModel)
                //&& EqualityComparer<Type>.Default.Equals(TypeToWrap, other.TypeToWrap)
                && EqualityComparer<IPropFactory>.Default.Equals(PropFactory, other.PropFactory)
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

                //if (TypeToWrap != null)
                //{
                //    hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TypeToWrap);
                //}

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

        #endregion

        #region = / != operators for MapTypeDefinition<T>

        ///<remarks>
        /// We are not a IMapTypeDefinitionGen and so we cannot implement these operators for the
        /// IMapTypeDefinitionGen interface.
        ///</remarks>


        public static bool operator ==(MapTypeDefinition<T> definition1, MapTypeDefinition<T> definition2)
        {
            return EqualityComparer<MapTypeDefinition<T>>.Default.Equals(definition1, definition2);
        }

        public static bool operator !=(MapTypeDefinition<T> definition1, MapTypeDefinition<T> definition2)
        {
            return !(definition1 == definition2);
        }

        #endregion operators

        #endregion IEquatable Support and Object Overrides
    }
}
