using DRM.PropBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRM.PropBag.ControlModel;

namespace PropBagTestApp.ViewModels
{
    public class ReferenceBindViewModelPB : PropBagMin
    {

        public ReferenceBindViewModelPB() : base(PropBagTypeSafetyMode.None, new PropFactory(true, GetTypeFromName))
        {
            System.Diagnostics.Debug.WriteLine("ReferenceBindViewModelPB is being created no params - with Type Factory.");
        }

        public ReferenceBindViewModelPB(byte dummy)
            : base(dummy)
        {
        }

        public ReferenceBindViewModelPB(PropModel pm, IPropFactory propFactory = null)
            : base(pm, propFactory)
        {

        }

        public ReferenceBindViewModelPB(PropBagTypeSafetyMode typeSafetyMode)
            : base(typeSafetyMode)
        {
        }

        public ReferenceBindViewModelPB(PropBagTypeSafetyMode typeSafetyMode, IPropFactory thePropFactory)
            : base(typeSafetyMode, thePropFactory)
        {
        }

        //public new object this[string typeName, string propertyName]
        //{
        //    get
        //    {
        //        return base[typeName, propertyName];
        //    }
        //    set
        //    {
        //        base[typeName, propertyName] = value;
        //    }
        //}

        public static Type GetTypeFromName(string typeName)
        {
            Type result;
            try
            {
                result = Type.GetType(typeName);
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", e);
            }

            if (result == null)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.");
            }

            return result;
        }
    }


}

