using DRM.TypeSafePropertyBag;
using System;
using System.Windows.Data;

using System.Collections.ObjectModel;

namespace DRM.PropBagWPF
{
    using PropIdType = UInt32;
    using PropNameType = String;

    public class CViewProp<T> : PropTypedBase<ListCollectionView>
    {
        private PropNameType _propertyName { get; set; }

        public CViewProp(PropNameType propertyName)
            : base(typeof(ListCollectionView), true, true, true, RefEqualityComparer<ListCollectionView>.Default.Equals, null, PropKindEnum.CollectionView)
        {
            _propertyName = propertyName;
            TypedValue = null;
        }

        override public ListCollectionView TypedValue { get; set; }

        public override object TypedValueAsObject => TypedValue;

        public override object Clone() => throw new NotSupportedException($"{nameof(CViewProp<T>)} Prop Items do not implement the Clone method.");

        public override void CleanUpTyped()
        {
            if (TypedValue is IDisposable disable)
            {
                disable.Dispose();
            }
        }
    }
}
         