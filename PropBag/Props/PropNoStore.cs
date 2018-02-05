using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public class PropNoStore<T> : PropTypedBase<T>
    {
        public PropNoStore(
            Func<string, T> defaultValFunc,
            bool typeIsSolid,
            PropStorageStrategyEnum storageStrategy,
            Func<T, T, bool> comparer)
            : base(typeof(T), typeIsSolid, CheckStorageStrategy(storageStrategy), false, comparer, defaultValFunc, PropKindEnum.Prop)
        {
        }

        static PropStorageStrategyEnum CheckStorageStrategy(PropStorageStrategyEnum strategy)
        {
            if (strategy == PropStorageStrategyEnum.Internal)
            {
                throw new InvalidOperationException($"This implementation of IProp<T> does not support the {nameof(PropStorageStrategyEnum.Internal)} StorageStrategy.");
            }
            return strategy;
        }

        T _value = default(T);
        override public T TypedValue 
        {
            get => _value;

            set
            {
                // It's as if this call never happened.
            }
        }

        public override object Clone()
        {
            PropNoStore<T> result = new PropNoStore<T>(this.GetDefaultValFunc, TypeIsSolid, StorageStrategy, Comparer);

            return result;
        }


        public override void CleanUpTyped()
        {
            // There's nothing to clean up.
        }

    }
}
         