using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DRM.TypeWrapper
{
    class SimpleModuleBuilderInfo
    {
        //private static readonly byte[] PRIVATE_KEY =
        //    StringToByteArray(
        //        "002400000480000094000000060200000024000052534131000400000100010079dfef85ed6ba841717e154f13182c0a6029a40794a6ecd2886c7dc38825f6a4c05b0622723a01cd080f9879126708eef58f134accdc99627947425960ac2397162067507e3c627992aa6b92656ad3380999b30b5d5645ba46cc3fcc6a1de5de7afebcf896c65fb4f9547a6c0c6433045fceccb1fa15e960d519d0cd694b29a4");

        //private static readonly byte[] PRIVATE_KEY_TOKEN = StringToByteArray("be96cd2c38ef1005");


        private static readonly string ASSEMBLY_NAME = "DRM.WrapperGenerator.Wrappers";
        private static readonly string MODULE_NAME = "Module_DRM.WrapperGenerator.Wrappers";

        private Lazy<AssemblyName> _assemblyName;
        private Lazy<AssemblyBuilder> _assemblyBuilder;
        private Lazy<ModuleBuilder> _moduleBuilder;


        public SimpleModuleBuilderInfo()
        {
            _assemblyName = new Lazy<AssemblyName>(() => GetAssemblyName(ASSEMBLY_NAME), LazyThreadSafetyMode.ExecutionAndPublication);
            _assemblyBuilder = new Lazy<AssemblyBuilder>(() => GetAssemblyBuilder(_assemblyName.Value));
            _moduleBuilder = new Lazy<ModuleBuilder>(() => GetModuleBuilder(_assemblyBuilder.Value, MODULE_NAME), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public int AssemblyId
        {
            get
            {
                return AssemblyIdIssuer.NextModuleId;
            }
        }

        public AssemblyName AssemblyName => _assemblyName.Value;

        public AssemblyBuilder AssemblyBuilder => _assemblyBuilder.Value;

        public ModuleBuilder ModuleBuilder => _moduleBuilder.Value;


        private AssemblyName GetAssemblyName(string assemblyName)
        {
            AssemblyName result = new AssemblyName(assemblyName);
            //result.SetPublicKey(privateKey);
            //result.SetPublicKeyToken(privateKeyToken);
            return result;
        }

        private AssemblyBuilder GetAssemblyBuilder(AssemblyName assemblyName)
        {
#if NET40
            return AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#else
            return AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
        }

        private ModuleBuilder GetModuleBuilder(AssemblyBuilder assemblyBuilder, string moduleName)
        {
            return assemblyBuilder.DefineDynamicModule(moduleName);
        }

        private byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

    }
}
