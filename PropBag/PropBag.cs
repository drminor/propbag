using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace DRM.PropBag
{
    #region Summary and Remarks

    /// <summary>
    /// The contents of this code file were designed and created by David R. Minor, Pittsboro, NC.
    /// I have chosen to provide others free access to this intellectual property using the terms set forth
    /// by the well known Code Project Open License.
    /// Please refer to the file in this same folder named CPOP.htm for the exact set of terms that govern this release.
    /// Although not included as a condition of use, I would prefer that this text, 
    /// or a similar text which covers all of the points made here, be included along with a copy of cpol.htm
    /// in the set of artifacts deployed with any product
    /// wherein this source code, or a derivative thereof, is used.
    /// </summary>

    #endregion

    public class PropBag : PropBagBase
    {
        #region Constructor

        protected PropBag() { }

        protected PropBag(byte dummy) : base(dummy) {}

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode) {}

        protected PropBag(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory thePropFactory) : base(typeSafetyMode, thePropFactory) {}

        protected PropBag(DRM.PropBag.ControlModel.PropModel pm) : base(pm) { }

        #endregion

    }
}
