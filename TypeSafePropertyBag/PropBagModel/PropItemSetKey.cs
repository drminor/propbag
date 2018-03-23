using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public class PropItemSetKey<L2TRaw> : IEquatable<PropItemSetKey<L2TRaw>>
    {
        public readonly WeakRefKey<IPropModel<L2TRaw>> PropModel_wrKey;
        public readonly string FullClassName;
        public readonly long GenerationId;
        public readonly bool IsFixed;

        public PropItemSetKey()
        {
            PropModel_wrKey = default(WeakRefKey<IPropModel<L2TRaw>>);
            FullClassName = null;
            GenerationId = 0;
            IsFixed = false;
        }

        public PropItemSetKey(IPropModel<L2TRaw> propModel)
        {
            PropModel_wrKey = new WeakRefKey<IPropModel<L2TRaw>>(propModel);
            FullClassName = propModel.FullClassName ?? throw new ArgumentNullException("The propModel must have a non-null FullClassName.");
            GenerationId = propModel.GenerationId;
            IsFixed = propModel.IsFixed;

            _hashCode = ComputetHashCode();
        }

        public static PropItemSetKey<L2TRaw> Empty
        {
            get
            {
                PropItemSetKey<L2TRaw> result = new PropItemSetKey<L2TRaw>();
                return result;
            }
        }

        public bool IsEmpty => FullClassName == null;


        #region IEquatable and Object Overrides

        public override string ToString()
        {
            return $"PropItemSetKey: {FullClassName}, {GenerationId}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropItemSetKey<L2TRaw>);
        }

        public bool Equals(PropItemSetKey<L2TRaw> other)
        {
            //return other != null &&
            //       FullClassName == other.FullClassName &&
            //       GenerationId == other.GenerationId;


            //if (other == null) return false;

            //if(PropModel_wrKey == null && other.PropModel_wrKey == null)
            //{
            //    return true;
            //}

            //if(PropModel_wrKey == null || other.PropModel_wrKey == null)
            //{
            //    return false;
            //}

            //return PropModel_wrKey == other.PropModel_wrKey;

            return (!ReferenceEquals(other, null)) && PropModel_wrKey == other.PropModel_wrKey;
        }

        int _hashCode;
        public override int GetHashCode()
        {
            return _hashCode;
        }

        public int ComputetHashCode()
        {
            //var hashCode = -1269708783;
            //hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullClassName);
            //hashCode = hashCode * -1521134295 + GenerationId.GetHashCode();
            //return hashCode;

            // Two PropModels are the same if and only if they refer to the same instance.
            // It is up to the client to cache PropModels and to always present the same instance.
            object target = PropModel_wrKey.Target;
            int hashCode = target.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(PropItemSetKey<L2TRaw> key1, PropItemSetKey<L2TRaw> key2)
        {
            //return EqualityComparer<PropItemSetKey<L2TRaw>>.Default.Equals(key1, key2);
            return key1.Equals(key2);
        }

        public static bool operator !=(PropItemSetKey<L2TRaw> key1, PropItemSetKey<L2TRaw> key2)
        {
            return !(key1 == key2);
        }

        #endregion
    }
}
