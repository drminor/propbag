﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{

    public delegate object CreatePropWithValueDelegate(AbstractPropFactory propFactory,
        object value,
        string propertyName, object extraInfo, 
        bool hasStorage, bool isTypeSolid);

    public delegate object CreatePropDelegate(AbstractPropFactory propFactory,
        string propertyName, object extraInfo, 
        bool hasStorage, bool isTypeSolid);

}