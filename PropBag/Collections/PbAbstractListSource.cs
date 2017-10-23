using System;
using System.Collections;
using System.ComponentModel;

namespace DRM.PropBag.Collections
{
    public abstract class PbAbstractListSource : IListSource
    {
        protected virtual Func<object, IList> ListGetter { get; set;}
        protected virtual object Component { get; set; }

        public virtual bool ContainsListCollection => false;

        public PbAbstractListSource(Func<object, IList> listGetter, object component) 
        {
            ListGetter = listGetter;
            Component = component;
        }

        protected virtual IList GetList(object component)
        {
            return ListGetter(Component);
        }

        public virtual IList GetList()
        {
            return GetList(Component);
        }
    }
}
