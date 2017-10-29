using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    public class MapTypeDefinition<T> : IMapTypeDefinition<T>, IEquatable<IMapTypeDefinitionGen>, IEquatable<MapTypeDefinition<T>>
    {
        public MapTypeDefinition()
        {
            Type = typeof(T);
            IsPropBag = false;
            PropModel = null;
            NewWrapperType = null;
        }

        public MapTypeDefinition(PropModel pm, Type baseType)
        {
            Type = typeof(T);
            IsPropBag = true;
            PropModel = pm;
            NewWrapperType = baseType;
        }

        public Type Type { get; }

        public bool IsPropBag { get; }
        public PropModel PropModel { get; }
        public Type NewWrapperType { get; }

        private static IMapTypeDefinition<T> GetTypeDef(PropModel pm, Type baseType)
        {
            if (typeof(T).IsPropBagBased())
            {
                return new MapTypeDefinition<T>(pm, baseType);
            }
            else
            {
                return new MapTypeDefinition<T>();
            }
        }

        public override string ToString()
        {
            if(IsPropBag)
            {
                return $"MapTypeDef for IPropBag type: {Type.ToString()}, with {PropModel.Props.Count} properties.";
            }
            else
            {
                return $"MapTypeDef for 'regular' type: {Type.ToString()}";
            }
        }

        public override bool Equals(object obj)
        {
            var definition = obj as MapTypeDefinition<T>;
            return definition != null &&
                   EqualityComparer<Type>.Default.Equals(Type, definition.Type) &&
                   IsPropBag == definition.IsPropBag &&
                   EqualityComparer<PropModel>.Default.Equals(PropModel, definition.PropModel) &&
                   EqualityComparer<Type>.Default.Equals(NewWrapperType, definition.NewWrapperType);
        }

        bool IEquatable<MapTypeDefinition<T>>.Equals(MapTypeDefinition<T> other)
        {
            return other != null &&
                   EqualityComparer<Type>.Default.Equals(Type, other.Type) &&
                   IsPropBag == other.IsPropBag &&
                   EqualityComparer<PropModel>.Default.Equals(PropModel, other.PropModel) &&
                   EqualityComparer<Type>.Default.Equals(NewWrapperType, other.NewWrapperType);
        }

        #region IEquatable<MapTypeDefinition<T>>


        #endregion

        public override int GetHashCode()
        {
            var hashCode = 1110551584;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + IsPropBag.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<PropModel>.Default.GetHashCode(PropModel);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(NewWrapperType);
            return hashCode;
        }

        #region IEquatable<IMapTypeDefinitionGen>

        bool IEquatable<IMapTypeDefinitionGen>.Equals(IMapTypeDefinitionGen other)
        {
            return other != null &&
                   EqualityComparer<Type>.Default.Equals(Type, other.Type) &&
                   IsPropBag == other.IsPropBag &&
                   EqualityComparer<PropModel>.Default.Equals(PropModel, other.PropModel) &&
                   EqualityComparer<Type>.Default.Equals(NewWrapperType, other.NewWrapperType);
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

        #endregion
    }
}
