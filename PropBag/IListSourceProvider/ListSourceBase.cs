using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;

namespace DRM.PropBag.IListSourceProvider
{
    public class ListSourceBase<T> : ObservableCollection<T>, IListSource,  ITypedList
    {
        #region IListSource members
        public bool ContainsListCollection => false;



        public IList GetList()
        {
            return this;
        }


        #endregion

        #region ITypedList members

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            throw new NotImplementedException();
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            throw new NotImplementedException();
        }

        #endregion



    }
}
