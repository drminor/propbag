using System;
using System.Collections.Generic;
using System.Threading;


namespace DRM.TypeSafePropertyBag
{
    using static DRM.TypeSafePropertyBag.TypeExtensions.TypeExtensions;
    using PropNameType = String;

    public class PropTemplateTyped<T> : IPropTemplate<T>, IEquatable<PropTemplateTyped<T>>
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

        public Type PropFactoryType { get; }

        public bool ComparerIsDefault  { get; }
        public bool ComparerIsRefEquality { get; }

        public Delegate ComparerProxy => Comparer;

        public bool DefaultValFuncIsDefault => true;
        public object GetDefaultValFuncProxy => GetDefaultVal;

        public DoSetDelegate DoSetDelegate { get; set; }

        //public ActivationInfo ActivationInfo { get; set; }

        public Func<PropNameType, object, bool, IPropTemplate, IProp> PropCreator { get; set; }

        #endregion

        #region Constructors

        public PropTemplateTyped(PropKindEnum propKind, PropStorageStrategyEnum storageStrategy,
            Type propFactoryType, bool comparerIsDefault, bool comparerIsRefEquality,
            Func<T, T, bool> comparer, Func<string, T> defaultValFunc, bool defaultValFuncIsDefault)
        {
            PropKind = propKind;
            Type = typeof(T);
            StorageStrategy = storageStrategy;
            Attributes = new Attribute[] { };

            ComparerIsDefault = comparerIsDefault;
            ComparerIsRefEquality = comparerIsRefEquality;
            Comparer = comparer;
            GetDefaultVal = defaultValFunc;

            PropFactoryType = propFactoryType;

            _hashCode = ComputetHashCode();
        }

        #endregion

        #region Methods to Raise Events

        protected void OnValueChanged()
        {
            Interlocked.CompareExchange(ref ValueChanged, null, null)?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IEquatable Support

        int _hashCode;
        public override int GetHashCode()
        {
            return _hashCode;
        }

        private int ComputetHashCode()
        {
            var hashCode = 112535552;
            hashCode = hashCode * -1521134295 + PropKind.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + StorageStrategy.GetHashCode();
            hashCode = hashCode * -1521134295 + ComparerIsDefault.GetHashCode();
            hashCode = hashCode * -1521134295 + ComparerIsRefEquality.GetHashCode();
            hashCode = hashCode * -1521134295 + DefaultValFuncIsDefault.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IPropTemplate);
        }

        public bool Equals(PropTemplateTyped<T> other)
        {
            return Equals(other as IPropTemplate);
        }

        public bool Equals(IPropTemplate other)
        {
            if (other == null) return false;

            if(PropKind == other.PropKind &&
                EqualityComparer<Type>.Default.Equals(Type, other.Type) &&
                StorageStrategy == other.StorageStrategy &&
                DefaultValFuncIsDefault == other.DefaultValFuncIsDefault &&
                ComparerIsDefault == other.ComparerIsDefault &&
                ComparerIsRefEquality == other.ComparerIsRefEquality)
            {

                if(!DefaultValFuncIsDefault)
                {
                    if(!ReferenceEquals(GetDefaultValFuncProxy, other.GetDefaultValFuncProxy))
                        return false;
                }
                else
                {
                    if (!EqualityComparer<Type>.Default.Equals(PropFactoryType, other.PropFactoryType))
                        return false;
                }

                if (!ComparerIsDefault && !ComparerIsRefEquality)
                {
                    return ReferenceEquals(ComparerProxy, other.ComparerProxy);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(PropTemplateTyped<T> typed1, PropTemplateTyped<T> typed2)
        {
            return EqualityComparer<PropTemplateTyped<T>>.Default.Equals(typed1, typed2);
        }

        public static bool operator !=(PropTemplateTyped<T> typed1, PropTemplateTyped<T> typed2)
        {
            return !(typed1 == typed2);
        }

        //public override bool Equals(object obj)
        //{
        //    return Equals(obj as IPropTemplate);
        //}

        //public bool Equals(PropTemplateTyped<T> other)
        //{
        //    return Equals(other as IPropTemplate);
        //}

        //public bool Equals(IPropTemplate other)
        //{
        //    if(other != null)
        //    {
        //        bool te = EqualityComparer<Type>.Default.Equals(Type, other.Type);
        //        bool ae = EqualityComparer<Attribute[]>.Default.Equals(Attributes, other.Attributes);
        //        bool cpe = EqualityComparer<object>.Default.Equals(ComparerProxy, other.ComparerProxy);
        //        bool gdvfpe = EqualityComparer<object>.Default.Equals(GetDefaultValFuncProxy, other.GetDefaultValFuncProxy);

        //        bool aae = te && ae && cpe && gdvfpe;
        //    }


        //    bool result = other != null &&
        //           PropKind == other.PropKind &&
        //           EqualityComparer<Type>.Default.Equals(Type, other.Type) &&
        //           StorageStrategy == other.StorageStrategy &&
        //           EqualityComparer<Attribute[]>.Default.Equals(Attributes, other.Attributes) &&
        //           EqualityComparer<object>.Default.Equals(ComparerProxy, other.ComparerProxy) &&
        //           EqualityComparer<object>.Default.Equals(GetDefaultValFuncProxy, other.GetDefaultValFuncProxy);

        //    return result;
        //}

        //public override int GetHashCode()
        //{
        //    var hashCode = -1642754185;
        //    hashCode = hashCode * -1521134295 + PropKind.GetHashCode();
        //    hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
        //    hashCode = hashCode * -1521134295 + StorageStrategy.GetHashCode();
        //    hashCode = hashCode * -1521134295 + EqualityComparer<Func<T, T, bool>>.Default.GetHashCode(Comparer);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<Func<string, T>>.Default.GetHashCode(GetDefaultVal);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(PropFactoryType);
        //    hashCode = hashCode * -1521134295 + ComparerIsDefault.GetHashCode();
        //    hashCode = hashCode * -1521134295 + ComparerIsRefEquality.GetHashCode();
        //    hashCode = hashCode * -1521134295 + DefaultValFuncIsDefault.GetHashCode();
        //    return hashCode;
        //}



        #endregion
    }
}
         