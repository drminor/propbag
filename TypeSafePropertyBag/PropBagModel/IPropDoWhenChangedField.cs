using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropDoWhenChangedField
    {
        Type DeclaringType { get; set; }
        string FullClassName { get; set; }
        string InstanceKey { get; set; }
        MethodInfo Method { get; set; }
        bool MethodIsLocal { get; set; }
        string MethodName { get; set; }
        SubscriptionPriorityGroup PriorityGroup { get; set; }
        SubscriptionKind SubscriptionKind { get; set; }
        object Target { get; set; }
    }
}