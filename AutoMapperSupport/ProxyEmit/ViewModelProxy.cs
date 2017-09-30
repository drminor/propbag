using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport.ProxyEmit
{
    class ViewModelProxy
    {
        Type DtViewModelType { get; }
        Type RtViewModelType { get; }

        public ViewModelProxy(Type dtViewModelType, Type rtViewModelType)
        {
            DtViewModelType = dtViewModelType;
            RtViewModelType = rtViewModelType;
        }

    }
}
