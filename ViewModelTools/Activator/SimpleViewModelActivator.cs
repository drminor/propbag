using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.ViewModelTools
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class SimpleViewModelActivator : IViewModelActivator
    {
        #region Constructor 

        public SimpleViewModelActivator()
        {
        }

        #endregion

        #region IViewModelActivator Interface

        public bool HasPropModelLookupService => false;

        // BaseType + PropModel (BaseType known at compile time.)
        public object GetNewViewModel<BT>(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory = null, string fullClassName = null) where BT : class, IPropBag
        {
            //object[] parameters = new object[] { propModel, storeAccessCreator, propFactory, fullClassName };
            //object result = Activator.CreateInstance(typeof(BT), args: parameters);

            object result = GetNewViewModel(propModel, storeAccessCreator, typeof(BT), propFactory, fullClassName);
            return result;
        }

        // BaseType + PropModel (BaseType known only at run time.
        public object GetNewViewModel(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator, Type typeToCreate, IPropFactory propFactory = null, string fullClassName = null)
        {
            object[] parameters = new object[] { propModel, storeAccessCreator, propFactory ?? propModel.PropFactory, fullClassName ?? propModel.FullClassName };

            //BindingFlags bFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance;
            //object result = Activator.CreateInstance(typeToCreate, bFlags, binder: null, args: parameters, culture: null);

            object result = Activator.CreateInstance(typeToCreate, args: parameters);
            return result;
        }

        public object GetNewViewModel<BT>(IPropBagInternal copySource) where BT : class, IPropBag
        {
            //object[] parameters = new object[] { copySource };
            //object result = Activator.CreateInstance(typeof(BT), args: parameters);

            object result = GetNewViewModel(typeof(BT), copySource);

            return result;
        }

        public object GetNewViewModel(Type typeToCreate, IPropBagInternal copySource)
        {
            object[] parameters = new object[] { copySource };
            object result = Activator.CreateInstance(typeToCreate, args: parameters);
            return result;
        }

        #endregion
    }
}
