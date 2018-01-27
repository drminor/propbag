using System;

namespace DRM.PropBagControlsWPF
{
    /// <summary>
    /// Used to identify a property as returning an object that should have properties created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PropBagInstanceAttribute : System.Attribute 
    {
        public readonly string Description;
        public readonly string InstanceKey;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description">Any description you would like to use to identify or othewise document the property
        /// to which this attribute is applied.</param>
        public PropBagInstanceAttribute(string description) : this(ReflectionHelpers.DEFAULT_INSTANCE_KEY, description) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceKey">The instance key from the PropBagTemplate that must be matched.</param>
        /// <param name="description">Any description you would like to use to identify or othewise documeent the property
        /// to which this attribute is applied.</param>
        public PropBagInstanceAttribute(string instanceKey, string description)
        {
            InstanceKey = instanceKey;
            Description = description;
        }
    }


}





