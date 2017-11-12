using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    internal delegate bool DoSetDelegate(IPropBag target, uint propId, string propertyName, IProp prop, object value);

    // TODO: use the IPropBag interface instead of PropBag (concrete implementation.)
    internal delegate IList GetTypedCollectionDelegate(PropBag source, string propertyName);
}
