using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.PropBag.ControlsWPF.WPFHelpers
{
    public static class EnumHelpers_NotUsed
    {
        public static IList<EnumT> GetValues<EnumT>()
        {
            IEnumerable<EnumT> temp = (EnumT[])Enum.GetValues(typeof(EnumT));
            temp = temp.OrderByDescending(x => x);
            return temp.ToList();
        }

        public static IList<ENumT> GetValuesExcept<ENumT>(IEnumerable<ENumT> skipList)
        {
            IList<ENumT> all = GetValues<ENumT>();
            IEnumerable<ENumT> result = all.Except(skipList);
            return result.ToList();
        }

        public static IList<ENumT> GetValuesGreaterThan<ENumT>(ENumT threshold) where ENumT : IComparable<ENumT>
        {
            IList<ENumT> all = GetValues<ENumT>();
            IEnumerable<ENumT> result = all.Where(x => x.CompareTo(threshold) > 0);
            return result.ToList();
        }
    }
}
