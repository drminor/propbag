using AutoMapper;
using DRM.PropBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.AutoMapperSupport
{
    public delegate object MapFromX(IPropBag source);
    public delegate IPropBag MapToX(object source, IPropBag destination);
    public delegate IPropBag MapToNewX(object source);

    public interface IPropBagMapperGen
    {
        TypePair TypePair { get; }
        //MapFromX MapFrom { get; }
        //MapToX MapTo { get; }
        //MapToNewX MapToNew { get; }

        IMapperConfigurationExpression Configure(IMapperConfigurationExpression cfg);
        IMapper Mapper { get; set; }
    }
}
