using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

namespace PropBagTestApp
{
    public class PropBagProfile : Profile
    {
        public PropBagProfile(string profileName, Action<IProfileExpression> configurationAction) : base(profileName, configurationAction)
        {
            this.ShouldMapField = null;
        }
    }
}
