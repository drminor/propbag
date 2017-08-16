using System;

namespace DRM.PropBag.ControlsWPF
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PropBagInstanceAttribute : System.Attribute 
    {
        public readonly string PropBagTemplate;

       public PropBagInstanceAttribute(string propBagTemplate)
       {
           PropBagTemplate = propBagTemplate;
       }
    }
}





