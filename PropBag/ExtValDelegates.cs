using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public delegate T GetExtVal<T>(Guid tag);
    public delegate void SetExtVal<T>(Guid tag, T value);


    // Generic versions of above the above.
    public delegate object GetExtVal(Guid tag);
    public delegate void SetExtVal(Guid tag, object value);
}
