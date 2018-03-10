using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropBagLib.Tests.SpecBasedVMTests.TypeDescriptorTests
{
    public class TypeInspectorUtility
    {
        public static List<string> GetPropertyNames(object x)
        {
            List<string> result = new List<string>();

            ICustomTypeDescriptor ictd = GetCustomTypeDescriptor(x);

            if(ictd != null)
            {
                PropertyDescriptorCollection pdc = ictd.GetProperties();

                foreach(PropertyDescriptor pd in pdc)
                {
                    result.Add(pd.Name);
                }
            }

            return result;
        }

        public static ICustomTypeDescriptor GetCustomTypeDescriptor(object x)
        {
            if(x is ICustomTypeDescriptor ictd)
            {
                return ictd;
            }

            return null;
        }
    }
}
