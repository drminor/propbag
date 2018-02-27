using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.ViewModelTools
{
    using DRM.PropBag.AutoMapperSupport;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class SimpleViewModelActivator : IViewModelActivator
    {
        #region Constructor 

        public SimpleViewModelActivator()
        {
        }

        #endregion

        #region IViewModelActivator Interface

        // BaseType + PropModel (BaseType known at compile time.)
        public object GetNewViewModel<BT>(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            IPropFactory propFactory, string fullClassName) where BT : class, IPropBag
        {
            IProvideAutoMappers autoMapperService = null;
            object result = GetNewViewModel(typeof(BT), propModel, storeAccessCreator, autoMapperService, propFactory, fullClassName);
            return result;
        }

        // With AutoMapper Support
        // BaseType + PropModel (BaseType known at compile time.)
        object IViewModelActivator.GetNewViewModel<BT>(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
        {
            object result = GetNewViewModel(typeof(BT), propModel, storeAccessCreator, autoMapperService, propFactory, fullClassName);
            return result;
        }

        // BaseType + PropModel (BaseType known only at run time.)
        public object GetNewViewModel(Type typeToCreate, IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            IPropFactory propFactory, string fullClassName)
        {
            IProvideAutoMappers autoMapperService = null;
            object result = GetNewViewModel(typeToCreate, propModel, storeAccessCreator, autoMapperService, propFactory, fullClassName);
            return result;
        }

        // With AutoMapper Support
        // BaseType + PropModel (BaseType known only at run time.)
        public object GetNewViewModel(Type typeToCreate, IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
        {
            object[] parameters = new object[] { propModel, storeAccessCreator, autoMapperService, propFactory, fullClassName };

            //BindingFlags bFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance;
            //object result = Activator.CreateInstance(typeToCreate, bFlags, binder: null, args: parameters, culture: null);

            object result = Activator.CreateInstance(typeToCreate, args: parameters);
            return result;
        }

        public object GetNewViewModel<BT>(IPropBag copySource) where BT : class, IPropBag
        {
            object result = GetNewViewModel(typeof(BT), copySource);
            return result;
        }

        public object GetNewViewModel(Type typeToCreate, IPropBag copySource)
        {
            object[] parameters = new object[] { copySource };
            object result = Activator.CreateInstance(typeToCreate, args: parameters);
            return result;
        }

        #endregion
    }
}
