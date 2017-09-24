using System;

namespace AutoMapper.ExtraMembers
{
    /// <summary>
    /// Used to identify a property, field or (no arg) method as being suppplied from the configuration, instead of
    /// natively from the declaring source or member type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class ExtraMemberAttribute : System.Attribute
    {
        public readonly string Description;
        public string StrategyKey { get; }

        public ExtraMemberAttribute(string description, string strategyKey = null)
        {
            Description = description;
            StrategyKey = strategyKey;
        }
    }


}






