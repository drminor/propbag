using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.Configuration
{
    public interface ITypeMapConfiguration
    {
        void Configure(TypeMap typeMap);
        MemberList MemberList { get; }
        Type SourceType { get; }
        Type DestinationType { get; }
        bool IsOpenGeneric { get; }
        TypePair Types { get; }
        ITypeMapConfiguration ReverseTypeMap { get; }
        IList<MemberInfo> ExtraSourceMembers { get; }
        IList<MemberInfo> ExtraDestMembers { get; }
    }
}