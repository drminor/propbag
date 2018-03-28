using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.ViewModelTools
{
    using PropModelType = IPropModel<String>;
    using ViewModelActivatorInterface = IViewModelActivator<UInt32, String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;


    public class SimpleViewModelActivator : ViewModelActivatorInterface
    {
        #region Constructor 

        public SimpleViewModelActivator()
        {
        }

        #endregion

        #region IViewModelActivator Interface

        //// BaseType + PropModel (BaseType known at compile time.)
        //public object GetNewViewModel<BT>(PropModelType propModel, PSAccessServiceCreatorInterface storeAccessCreator,
        //    IPropFactory propFactory = null, string fcnOverride = null) where BT : class, IPropBag
        //{
        //    IProvideAutoMappers autoMapperService = null;
        //    ICreateWrapperTypes wrapperTypeCreator = null;

        //    object result = GetNewViewModel(typeof(BT), propModel, storeAccessCreator, autoMapperService, wrapperTypeCreator, propFactory, fcnOverride);
        //    return result;
        //}

        //public object GetNewViewModel<BT>(PropModelType propModel, PSAccessServiceCreatorInterface storeAccessCreator,
        //    IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator,
        //    IPropFactory propFactory, string fcnOverride) where BT : class, IPropBag
        //{
        //    object result = GetNewViewModel(typeof(BT), propModel, storeAccessCreator, autoMapperService, wrapperTypeCreator, propFactory, fcnOverride);
        //    return result;
        //}

        //// BaseType + PropModel (BaseType known only at run time.
        //public object GetNewViewModel(Type typeToCreate, PropModelType propModel, PSAccessServiceCreatorInterface storeAccessCreator,
        //    IPropFactory propFactory = null, string fcncOverride = null)
        //{
        //    IProvideAutoMappers autoMapperService = null;
        //    ICreateWrapperTypes wrapperTypeCreator = null;
        //    object result = GetNewViewModel(typeToCreate, propModel, storeAccessCreator, autoMapperService, wrapperTypeCreator, propFactory, fcncOverride);
        //    return result;
        //}

        //public object GetNewViewModel(Type typeToCreate, PropModelType propModel, PSAccessServiceCreatorInterface storeAccessCreator,
        //    IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator,
        //    IPropFactory propFactory, string fcnOverride)
        //{
        //    object[] parameters = new object[] { propModel, storeAccessCreator, autoMapperService, wrapperTypeCreator, propFactory, fcnOverride };

        //    //BindingFlags bFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance;
        //    //object result = Activator.CreateInstance(typeToCreate, bFlags, binder: null, args: parameters, culture: null);

        //    object result = Activator.CreateInstance(typeToCreate, args: parameters);
        //    return result;
        //}

        // BaseType + PropModel (BaseType known at compile time.)
        public object GetNewViewModel<BT>(PropModelType propModel, ViewModelFactoryInterface viewModelFactory,
            object ams, IPropFactory propFactory = null, string fcnOverride = null) where BT : class, IPropBag
        {
            object result = GetNewViewModel(typeof(BT), propModel, viewModelFactory, ams, propFactory, fcnOverride);
            return result;
        }


        public object GetNewViewModel(Type typeToCreate, PropModelType propModel, ViewModelFactoryInterface viewModelFactory,
             object ams, IPropFactory propFactory, string fcnOverride)
        {
            object[] parameters = new object[] { propModel, viewModelFactory, ams, propFactory, fcnOverride };

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
