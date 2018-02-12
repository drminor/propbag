using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;

namespace MVVMApplication.ViewModel
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class PersonCollectionViewModel : PropBag
    {
        public PersonCollectionViewModel(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, propFactory, fullClassName)
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
