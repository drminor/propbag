using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.TypeDescriptors;
using System.Collections.Specialized;

namespace DRM.PropBag.Collections
{
    public class PbCollection<T> : ObservableCollection<T>,  ITypedList
    {
        #region Private Members

        private PropModel _propModel;
        private IPropFactory _propFactory;

        IMyTypeDescriptor _pbTypeDesc;
        IMyTypeDescriptor PbTypeDesc
        {
            get
            {
                if(_pbTypeDesc == null)
                {
                    _pbTypeDesc = new PropBagTypeDescriptor<PropBag>(_propModel, _propFactory, "testing");
                }
                return _pbTypeDesc;
            }
            set
            {
                _pbTypeDesc = value;
            }
        }

        #endregion


        //public override event NotifyCollectionChangedEventHandler CollectionChanged;

        //protected override event PropertyChangedEventHandler PropertyChanged;


        #region Constructors

        private PbCollection() { } // Disallow parameterless constructor

        public PbCollection(PropModel propModel, IPropFactory propFactory, string listName)
        {
            init(propModel, propFactory, listName);
        }

        public PbCollection(PropModel propModel, IPropFactory propFactory, string listName, List<T> list) : base(list)
        {
            init(propModel, propFactory, listName);
        }

        public PbCollection(PropModel propModel, IPropFactory propFactory, string listName, IEnumerable<T> collection) : base(collection)
        {
            init(propModel, propFactory, listName);
        }

        private void init(PropModel propModel, IPropFactory propFactory, string listName)
        {
            PbTypeDesc = null;
            _propModel = propModel ?? throw new ArgumentNullException(nameof(propModel));
            _propFactory = propFactory ?? throw new ArgumentNullException(nameof(propFactory));
            ListName = listName ?? throw new ArgumentNullException(nameof(listName));

            //base.CollectionChanged += PbCollection_CollectionChanged;
        }

        //private void PbCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    OurOnCollectionChanged(e);
        //}

        //private void OurOnCollectionChanged(NotifyCollectionChangedEventArgs e)
        //{
        //    if(this.CollectionChanged != null)
        //    {
        //        this.CollectionChanged(this, e);
        //    }
        //}

        #endregion

        public string ListName { get; private set; }

        #region ITypedList members

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (listAccessors != null) throw new NotSupportedException("PbCollection does not support the use of listAccessors.");

            PropertyDescriptorCollection propDescriptors = PbTypeDesc.GetChildProperties();
            return propDescriptors;
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            if (listAccessors != null) throw new NotSupportedException("PbCollection does not support the use of listAccessors.");

            return ListName;
        }

        #endregion
    }
}
