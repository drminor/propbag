using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

namespace DRM.TypeWrapper.TypeDesc
{
    public class TypeDescriptionLocalCache : ICacheTypeDescriptions
    {
        //ITypeDescriptionProvider _typeDescriptionProvider;

        LockingConcurrentDictionary<NewTypeRequest, TypeDescription> _typeDescriptions;

        public TypeDescriptionLocalCache(ITypeDescriptionProvider typeDescriptionProvider)
        {
            //_typeDescriptionProvider = typeDescriptionProvider;
            _typeDescriptions = new LockingConcurrentDictionary<NewTypeRequest, TypeDescription>
                (typeDescriptionProvider.GetTypeDescription);
        }

        public TypeDescription GetOrAdd(NewTypeRequest request)
        {
            return _typeDescriptions.GetOrAdd(request);
        }
    }
}
