using System;
using System.Threading;
using System.Reflection.Emit;
using System.Reflection;

namespace DRM.PropBag.ViewModelBuilder
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

        Type BuildVmProxyClass(TypeDescription typeDescription);

    }
}
