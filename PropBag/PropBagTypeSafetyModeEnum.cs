﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public enum PropBagTypeSafetyMode
    {
        Tight, //AllPropMustBeRegistered + OnlyTypedAccess.
        AllPropsMustBeRegistered,
        OnlyTypedAccess,
        Loose
    }

}
