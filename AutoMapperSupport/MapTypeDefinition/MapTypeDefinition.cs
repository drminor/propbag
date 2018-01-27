using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    public class MapTypeDefinition<T> : IMapTypeDefinition<T>, IEquatable<IMapTypeDefinitionGen>, IEquatable<MapTypeDefinition<T>>
    {
        public Type TargetType { get; }

        public bool IsPropBag { get; }
        public IPropModel PropModel { get; }
        public Type TypeToWrap { get; }
        public Type NewWrapperType { get; set; }

        IPropFactory _propFactory { get; }
        string _fullClassName { get; }

        // If specified, use the PropFactory provided by the caller instead of the one specified by the PropModel.
        public IPropFactory PropFactory => _propFactory ?? PropModel.PropFactory;

        // If specified, use the FullClassName provided by the caller instead of the one specified by the PropModel.
        public string FullClassName => _fullClassName ?? PropModel.FullClassName;

        #region Constructors

        // For non-IPropBag based types.
        public MapTypeDefinition()
        {
            TargetType = typeof(T);
            IsPropBag = false;
            PropModel = null;
            TypeToWrap = null;
            NewWrapperType = null;
            _fullClassName = null;
            _propFactory = null;
        }

        public MapTypeDefinition(IPropModel pm, Type typeToWrap, string fullClassName, IPropFactory propFactory)
        {
            if (typeToWrap != null && pm.TypeToCreate != typeToWrap)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: the value for the PropModel's TypeToCreate property is not equal to the supplied typeToWrap.");
            }

            TargetType = typeof(T);
            IsPropBag = true;
            PropModel = pm ?? throw new ArgumentNullException(nameof(pm));
            TypeToWrap = typeToWrap ?? pm.TypeToCreate; // throw new ArgumentNullException(nameof(typeToWrap));
            NewWrapperType = null;
            _fullClassName = fullClassName;
            _propFactory = propFactory;
        }

        #endregion

        // TODO: Check the IEquatable support for MapTypeDefinition.
        #region IEquatable Support and Object Overrides

        public override string ToString()
        {
            if(IsPropBag)
            {
                return $"MapTypeDef for IPropBag type: {TargetType.ToString()}, with {PropModel.Props.Count} properties.";
            }
            else
            {
                return $"MapTypeDef for 'regular' type: {TargetType.ToString()}";
            }
        }

        public override bool Equals(object obj)
        {
            var definition = obj as MapTypeDefinition<T>;
            return definition != null
                && EqualityComparer<Type>.Default.Equals(TargetType, definition.TargetType)
                && EqualityComparer<Type>.Default.Equals(TypeToWrap, definition.TypeToWrap);
        }

        bool IEquatable<MapTypeDefinition<T>>.Equals(MapTypeDefinition<T> other)
        {
            return other != null
                && EqualityComparer<Type>.Default.Equals(TargetType, other.TargetType)
                && EqualityComparer<Type>.Default.Equals(TypeToWrap, other.TypeToWrap);
        }

        #region IEquatable<MapTypeDefinition<T>>


        #endregion

        #region IEquatable<IMapTypeDefinitionGen>

        bool IEquatable<IMapTypeDefinitionGen>.Equals(IMapTypeDefinitionGen other)
        {
            return other != null
                && EqualityComparer<Type>.Default.Equals(TargetType, other.TargetType)
                && EqualityComparer<Type>.Default.Equals(TypeToWrap, other.TypeToWrap);
        }

        public override int GetHashCode()
        {
            var hashCode = 970652040;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TargetType);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TypeToWrap);
            return hashCode;
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
