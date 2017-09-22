using System;
using System.Threading;
using System.Reflection.Emit;
using System.Reflection;

namespace DRM.WrapperGenLib
{
    public interface IModuleBuilderInfoProvider
    {
        IModuleBuilderInfo ModuleBuilderInfo { get; }
    }

    public interface IModuleBuilderInfo
    {
        int AssemblyId { get; }
        AssemblyName AssemblyName { get; }
        AssemblyBuilder AssemblyBuilder { get; }
        ModuleBuilder ModuleBuilder { get; }

        Type GetWrapperType(TypeDescription typeDescription);

    }
}
