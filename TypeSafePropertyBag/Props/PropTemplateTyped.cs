using System;

namespace DRM.TypeSafePropertyBag
{
    public class PropTemplateTyped<T> : PropTemplate, IPropTemplate<T>
    {
        #region Public and Protected Properties

        public virtual T TypedValue { get; set; }
        public override bool ReturnDefaultForUndefined => GetDefaultValFunc != null;

        public Func<T, T, bool> Comparer { get; }
        public GetDefaultValueDelegate<T> GetDefaultValFunc { get; }

        #endregion

        #region Constructors

        protected PropTemplateTyped(Type typeOfThisValue, bool typeIsSolid, PropStorageStrategyEnum storageStrategy,
            Func<T,T,bool> comparer, GetDefaultValueDelegate<T> defaultValFunc, PropKindEnum propKind)
            : base(propKind, typeOfThisValue, typeIsSolid, storageStrategy)
        {
            Comparer = comparer;
            GetDefaultValFunc = defaultValFunc;
        }

        #endregion

        #region Public Methods

        public bool CompareTo(T newValue)
        {
            if(StorageStrategy == PropStorageStrategyEnum.Internal)
            {
                return Comparer(newValue, TypedValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool Compare(T val1, T val2)
        {
            //if (!ValueIsDefined) return false;

            return Comparer(val1, val2);
        }


        #endregion
    }
}
         