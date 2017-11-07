using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    internal delegate bool DoSetDelegate(object value, PropBag target, string propertyName, IProp prop);

    internal delegate IList GetTypedCollectionDelegate(PropBag source, string propertyName);
}
