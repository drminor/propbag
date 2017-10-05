using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ViewModelBuilder
{
    public class AttributeEmitter
    {
        // TODO: Make a general way of creating attributes.
        public CustomAttributeBuilder CreateAttributeBuilder(Attribute template)
        {
            Type[] ctorParams = new Type[] { typeof(string) };

            ConstructorInfo classCtorInfo
                = template.GetType().GetConstructor(ctorParams);


            CustomAttributeBuilder attibBuilder
                = new CustomAttributeBuilder(classCtorInfo,
                                new object[] { DateTime.Now.ToString() });

            return attibBuilder;
        }

    }
}
