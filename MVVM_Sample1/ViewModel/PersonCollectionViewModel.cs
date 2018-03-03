using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using System;


namespace MVVMApplication.ViewModel
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class PersonCollectionViewModel : PropBag
    {
        public PersonCollectionViewModel(PropModelType pm, PSAccessServiceCreatorInterface storeAccessCreator, IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, autoMapperService, propFactory, fullClassName)
        {
            System.Diagnostics.Debug.WriteLine("Constructing PersonCollectionViewModel -- with PropModel.");
        }

        protected PersonCollectionViewModel(PersonCollectionViewModel copySource)
            : base(copySource)
        {
        }

        new public object Clone()
        {
            return new PersonCollectionViewModel(this);
        }
    }
}
