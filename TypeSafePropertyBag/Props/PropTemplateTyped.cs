using System;
using System.Collections.Generic;
using System.Threading;

using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;

    public class PropTemplateTyped<T> : IPropTemplate<T>, IEquatable<IPropTemplate>, IEquatable<PropTemplateTyped<T>>
    {
        #region Public and Protected Properties

        public event EventHandler<EventArgs> ValueChanged;

        public PropKindEnum PropKind { get; protected set; }
        public Type Type { get; }
        public bool IsPropBag => Type.IsPropBagBased();

        public PropStorageStrategyEnum StorageStrategy { get; protected set; }

        public Attribute[] Attributes { get; }

        public Func<T, T, bool> Comparer { get; }
        public Func<string, T> GetDefaultVal { get; }

        public Delegate ComparerProxy => Comparer;
        public object GetDefaultValFuncProxy => GetDefaultVal;

        public DoSetDelegate DoSetDelegate { get; set; }

        //public ActivationInfo ActivationInfo { get; set; }

        public Func<PropNameType, object, bool, IPropTemplate, IProp> PropCreator { get; set; }

        #endregion

        #region Constructors

        public PropTemplateTyped(PropKindEnum propKind, PropStorageStrategyEnum storageStrategy,
            Func<T, T, bool> comparer, Func<string, T> defaultValFunc)
        {
            PropKind = propKind;
            Type = typeof(T);
            StorageStrategy = storageStrategy;
            Attributes = new Attribute[] { };

            Comparer = comparer;
            GetDefaultVal = defaultValFunc;

            _hashCode = ComputetHashCode();
        }

        #endregion

        #region Methods to Raise Events

        protected void OnValueChanged()
        {
            Interlocked.CompareExchange(ref ValueChanged, null, null)?.Invoke(this, EventArgs.Empty);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IPropTemplate);
        }

        public bool Equals(PropTemplateTyped<T> other)
        {
            return Equals(other as IPropTemplate);
            //return other != null &&
            //       PropKind == other.PropKind &&
            //       EqualityComparer<Type>.Default.Equals(Type, other.Type) &&
            //       StorageStrategy == other.StorageStrategy &&
            //       EqualityComparer<Attribute[]>.Default.Equals(Attributes, other.Attributes) &&
            //       EqualityComparer<object>.Default.Equals(ComparerProxy, other.ComparerProxy) &&
            //       EqualityComparer<object>.Default.Equals(GetDefaultValFuncProxy, other.GetDefaultValFuncProxy);
        }

        public bool Equals(IPropTemplate other)
        {
            return other != null &&
                   PropKind == other.PropKind &&
                   EqualityComparer<Type>.Default.Equals(Type, other.Type) &&
                   StorageStrategy == other.StorageStrategy &&
                   EqualityComparer<Attribute[]>.Default.Equals(Attributes, other.Attributes) &&
                   EqualityComparer<object>.Default.Equals(ComparerProxy, other.ComparerProxy) &&
                   EqualityComparer<object>.Default.Equals(GetDefaultValFuncProxy, other.GetDefaultValFuncProxy);
        }

        int _hashCode;
        public override int GetHashCode()
        {
            return _hashCode;
        }

        private int ComputetHashCode()
        {
            var hashCode = -1386958845;
            hashCode = hashCode * -1521134295 + PropKind.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + StorageStrategy.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Attribute[]>.Default.GetHashCode(Attributes);
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(ComparerProxy);
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(GetDefaultValFuncProxy);
            return hashCode;
        }

        public static bool operator ==(PropTemplateTyped<T> typed1, PropTemplateTyped<T> typed2)
        {
            return EqualityComparer<PropTemplateTyped<T>>.Default.Equals(typed1, typed2);
        }

        public static bool operator !=(PropTemplateTyped<T> typed1, PropTemplateTyped<T> typed2)
        {
            return !(typed1 == typed2);
        }

        #endregion
    }

    public class PropTemplateGenComparer : IEqualityComparer<IPropTemplate>
    {
        public bool Equals(IPropTemplate x, IPropTemplate y)
        {
            if(x is IEquatable<IPropTemplate> a)
            {
                return a.Equals(y);
            }
            return false; // throw new NotImplementedException("This item does not implement IEquatable<IPropTemplate>");
        }

        public int GetHashCode(IPropTemplate obj)
        {
            return obj.GetHashCode();
        }
    }
}
         