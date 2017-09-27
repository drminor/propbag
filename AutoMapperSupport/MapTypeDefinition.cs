using DRM.PropBag.ControlModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
    public class MapTypeDefinition<T> : IMapTypeDefinition<T>
    {
        public MapTypeDefinition()
        {
            Type = typeof(T);
            IsPropBag = false;
            PropModel = null;
            BaseType = null;
        }

        public MapTypeDefinition(PropModel pm, Type baseType)
        {
            Type = typeof(T);
            IsPropBag = true;
            PropModel = pm;
            BaseType = baseType;
        }

        public Type Type { get; }

        public bool IsPropBag { get; }
        public PropModel PropModel { get; }
        public Type BaseType { get; }
    }
}
