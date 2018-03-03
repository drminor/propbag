using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.ViewModelTools
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public interface IViewModelActivator
    {
        // Create new Type that is derived from a Type known only at run time.
        object GetNewViewModel(Type typeToCreate, PropModelType propModel,
            PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName);

        // With AutoMapper Support
        object GetNewViewModel(Type typeToCreate, PropModelType propModel,
            PSAccessServiceCreatorInterface storeAccessCreator, IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName);

        // Create new Type that is derived from a Type known at compile time.
        object GetNewViewModel<BT>(PropModelType propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            IPropFactory propFactory = null, string fullClassName = null) where BT : class, IPropBag;

        // With AutoMapper Support
        object GetNewViewModel<BT>(PropModelType propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName) where BT : class, IPropBag;

        object GetNewViewModel(Type typeToCreate, IPropBag copySource);
        object GetNewViewModel<BT>(IPropBag copySource) where BT : class, IPropBag;

    }
}
