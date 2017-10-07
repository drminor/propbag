using System;
using System.Collections.Generic;
using System.Reflection;
using DRM.TypeSafePropertyBag;
using DRM.PropBag;
using DRM.PropBag.ControlModel;

namespace DRM.PropBag.AutoMapperSupport
{
    /// <summary>
    /// Extends objects that implement the DRM.PropBag.IPropBag interface.
    /// </summary>
    public static class ExtraMembersPropModelExtensions
    {

        public static IEnumerable<MemberInfo> GetExtraMembers(this PropModel pm)
        {
            List<MemberInfo> result = new List<MemberInfo>();

            foreach (PropItem propItem in pm.Props)
            {
                string propertyName = propItem.PropertyName;
                Type propertyType = propItem.PropertyType;

                Func<ITypeSafePropBag, string, Type, object> getter =
                    new Func<ITypeSafePropBag, string, Type, object>((host, pn, pt) => PBDispatcher.GetValue(host, pn, pt));

                Action<ITypeSafePropBag, string, Type, object> setter =
                    new Action<ITypeSafePropBag, string, Type, object>((host, pn, pt, value) => PBDispatcher.SetValue(host, pn, pt, value));

                PropertyInfoWT pi = new PropertyInfoWT(propertyName, propertyType, typeof(PBDispatcher),
                    getter, setter);

                result.Add(pi);

            }

            return result;
        }





    }
}
