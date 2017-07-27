using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public delegate T GetExtVal<T>(Guid tag);
    public delegate void SetExtVal<T>(Guid tag, T value);
}
