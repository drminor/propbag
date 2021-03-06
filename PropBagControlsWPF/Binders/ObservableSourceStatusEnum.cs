﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBagControlsWPF.Binders
{
    public enum ObservableSourceStatusEnum
    {
        NoType = 0,
        HasType,
        Ready,
        IsWatchingProp,
        IsWatchingColl,
        IsWatchingPropAndColl,
        Undetermined // This is used for DataSourceProviders
    }

}
