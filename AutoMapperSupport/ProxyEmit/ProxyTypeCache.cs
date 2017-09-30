using DRM.PropBag.ViewModelBuilder;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport.ProxyEmit
{
    // TODO: This feature is already available via the 
    // DRM.PropBag.ViewModelBuilder.DefaultModuleBuilderInfoProvider
    // and 
    // DRM.PropBag.ViewModelBuilder.IModuleBuilderInfoProvider
    //
    // Consider using this type of implementation instead
    // and place it in the DRM.PropBag.ViewModelBuilder namespace.


    class ProxyTypeCache
    {
        private LockingConcurrentDictionary<ProxyRequest, ViewModelProxy> _builtProxies;

        public IModuleBuilderInfo ModuleBuilderInfo { get; }
         
        public ProxyTypeCache(IModuleBuilderInfo moduleBuilderInfo)
        {
            _builtProxies = new LockingConcurrentDictionary<ProxyRequest, ViewModelProxy>(BuildProxy);
            ModuleBuilderInfo = moduleBuilderInfo;
        }

        public ViewModelProxy GetOrAdd(ProxyRequest key)
        {
            return _builtProxies.GetOrAdd(key);
        }

        private ViewModelProxy BuildProxy(ProxyRequest key)
        {
            return null;
        }

    }
}
