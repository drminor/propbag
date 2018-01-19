using System.Reflection;
using System.Reflection.Emit;

namespace DRM.PropBag.TypeWrapper
{
    public interface IModuleBuilderInfo
    {
        int AssemblyId { get; }
        AssemblyName AssemblyName { get; }
        AssemblyBuilder AssemblyBuilder { get; }
        ModuleBuilder ModuleBuilder { get; }
    }
}
