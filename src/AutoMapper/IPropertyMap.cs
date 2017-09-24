using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    /// <summary>
    /// An object that implements this interface is supplied to the
    /// CreateMemberGetterStrategy and CreateMemberSetterStrategy Actions
    /// defined for a profile.
    /// It is not used to support various implementations of a PropertyMap
    /// </summary>
    public interface IPropertyMap
    {
        MemberInfo SourceMember { get; }
        Type SourceType { get; }

        MemberInfo DestinationProperty { get; }
        Type DestinationPropertyType { get; }

        LambdaExpression PreCondition { get; }
        LambdaExpression Condition { get; }

        bool AllowNull { get; set; }
        bool UseDestinationValue { get; set; }
        object NullSubstitute { get; set; }

        bool HasSource();
        bool Ignored { get; }
        bool IsMapped();

        bool Inline { get; set; }

    }
}