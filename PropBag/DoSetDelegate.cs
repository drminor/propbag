using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    internal delegate bool DoSetDelegate(object value, PropBagBase target, string propertyName, IPropGen prop);
}
