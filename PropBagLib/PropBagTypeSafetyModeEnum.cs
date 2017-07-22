using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropBagLib
{
    public enum PropBagTypeSafetyModeEnum
    {
        AllPropsMustBeRegistered,
        AllPropsMustBeFirstSetWithSetIt,
        Loose
    }

}
