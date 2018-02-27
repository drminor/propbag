﻿using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.ViewModelTools
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public interface IViewModelActivator
    {
        // Create new Type that is derived from a Type known only at run time.
        object GetNewViewModel(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            Type typeToCreate, IPropFactory propFactory = null, string fullClassName = null);

        // With AutoMapper Support
        object GetNewViewModel(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            Type typeToCreate, IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName);

        // Create new Type that is derived from a Type known at compile time.
        object GetNewViewModel<BT>(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            IPropFactory propFactory = null, string fullClassName = null) where BT : class, IPropBag;

        // With AutoMapper Support
        object GetNewViewModel<BT>(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService, IPropFactory propFactory = null, string fullClassName = null) where BT : class, IPropBag;

        object GetNewViewModel(Type typeToCreate, IPropBag copySource);
        object GetNewViewModel<BT>(IPropBag copySource) where BT : class, IPropBag;

    }
}
