using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICachePropBagMappers
    {
        void RegisterMapperRequest(IPropBagMapperKeyGen mapRequest);

        IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapRequest);
    }
}
