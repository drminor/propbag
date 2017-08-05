using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{

    public delegate object CreatePropDelegate(AbstractPropFactory propFactory, object value, bool hasStorage, bool isTypeSolid);

    public delegate object CreatePropWithNoneOrDefaultDelegate(AbstractPropFactory propFactory, bool hasStorage, bool isTypeSolid);

}
