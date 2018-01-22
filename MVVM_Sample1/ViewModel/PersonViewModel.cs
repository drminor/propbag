using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;

namespace MVVMApplication.ViewModel
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class PersonVM : PropBag, ICloneable
    {
        public PersonVM(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, propFactory, fullClassName)
        {
        }

        protected PersonVM(PersonVM copySource)
            : base(copySource)
        {
        }

        new public object Clone()
        {
            return new PersonVM(this);
        }

        //public override string ToString()
        //{
        //    IDictionary<string, ValPlusType> x = GetAllPropNamesAndTypes();

        //    StringBuilder result = new StringBuilder();
        //    int cnt = 0;
        //    foreach(KeyValuePair<string, ValPlusType> kvp in x)
        //    {
        //        if(cnt++  == 0) result.Append("\n\r");

        //        result.Append($" -- {kvp.Key}: {kvp.Value.Value}");
        //    }
        //    return result.ToString();
        //}
    }

  
}
