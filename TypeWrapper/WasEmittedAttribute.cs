using System;

namespace DRM.PropBag.TypeWrapper
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class WasEmittedAttribute : Attribute
    {
        private DateTime dateUpdated;
        public DateTime DateUpdated
        {
            get
            {
                return dateUpdated;
            }
        }

        public WasEmittedAttribute(string theDate)
        {
            dateUpdated = DateTime.Parse(theDate);
        }
    }

}
