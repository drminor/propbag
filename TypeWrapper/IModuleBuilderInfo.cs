using System.Reflection;
using System.Reflection.Emit;

namespace DRM.TypeWrapper
{
    public interface IModuleBuilderInfo
    {
        int AssemblyId { get; }
        AssemblyName AssemblyName { get; }
        AssemblyBuilder AssemblyBuilder { get; }
        ModuleBuilder ModuleBuilder { get; }
    }
}
