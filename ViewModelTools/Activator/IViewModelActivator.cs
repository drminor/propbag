﻿using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.TypeWrapper;
using System;

namespace DRM.PropBag.ViewModelTools
{
    public interface IViewModelActivator<L2T, L2TRaw>
    {
        // Create new Type that is derived from a Type known only at run time.
        object GetNewViewModel(Type typeToCreate, IPropModel<L2TRaw> propModel,
            IPropStoreAccessServiceCreator<L2T, L2TRaw> storeAccessCreator, IPropFactory pfOverride, string fcnOverride);

        // With AutoMapper Support
        object GetNewViewModel(Type typeToCreate, IPropModel<L2TRaw> propModel,
            IPropStoreAccessServiceCreator<L2T, L2TRaw> storeAccessCreator,
            IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator,
            IPropFactory pfOverride, string fcnOverride);

        // Create new Type that is derived from a Type known at compile time.
        object GetNewViewModel<BT>(IPropModel<L2TRaw> propModel, IPropStoreAccessServiceCreator<L2T, L2TRaw> storeAccessCreator,
            IPropFactory pfOverride, string fcnOverride) where BT : class, IPropBag;

        // With AutoMapper Support
        object GetNewViewModel<BT>(IPropModel<L2TRaw> propModel, IPropStoreAccessServiceCreator<L2T, L2TRaw> storeAccessCreator,
            IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator,
            IPropFactory pfOverride, string fcnOverride) where BT : class, IPropBag;

        object GetNewViewModel(Type typeToCreate, IPropBag copySource);
        object GetNewViewModel<BT>(IPropBag copySource) where BT : class, IPropBag;

    }
}
