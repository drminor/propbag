using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DRM.PropBag.TypeWrapper
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
