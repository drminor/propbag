using System;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.PropBagWPF.Unused
{
    public class CViewPropGen : CViewProp
    {
        public CViewPropGen(string propertyName, ICollectionView initialValue) : base(propertyName, null)
        {
            if (initialValue != null)
            {
                if (initialValue is ListCollectionView lcv)
                {
                    TypedValue = lcv;
                }
                else
                {
                    if (initialValue != null)
                    {
                        throw new ArgumentException($"The initialValue is not a ListCollectionView.", nameof(initialValue));
                    }
                }
            }
            else
            {
                TypedValue = null;
            }
        }
    }
}
         