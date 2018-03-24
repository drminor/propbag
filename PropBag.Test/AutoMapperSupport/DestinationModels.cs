﻿using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.TypeWrapper;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class DestinationModel3 : PropBag
    {
        //public DestinationModel3(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator,
        //    string fullClassName, IPropFactory propFactory)
        //    : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName)
        //{
        //    AddProp<Guid>("ProductId", null, null, Guid.NewGuid());
        //    AddProp<int>("Amount", null, null, initialValue: 0);
        //    AddProp<double>("Size", null, null, 10.1);
        //    AddProp<MyModel4>("Deep", null, null, null);
        //}

        public DestinationModel3(PropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator,
            IPropFactory propFactory, string fullClassName)
            : base(propModel, storeAccessCreator, autoMapperService, wrapperTypeCreator, propFactory, fullClassName)
        {
            //AddProp<Guid>("ProductId", null, null, Guid.NewGuid());
            //AddProp<int>("Amount", null, null, initialValue: 0);
            //AddProp<double>("Size", null, null, 10.1);
            //AddProp<MyModel4>("Deep", null, null, null);
        }

        public DestinationModel3(DestinationModel3 copySource)
            : base(copySource)
        {
        }

        override public object Clone()
        {
            return new DestinationModel3(this);
        }

    }
}
