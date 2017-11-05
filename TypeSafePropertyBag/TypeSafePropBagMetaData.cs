using System;
using System.Collections.Generic;
using System.Linq;
namespace DRM.TypeSafePropertyBag
{
    public class TypeSafePropBagMetaData
    {
        private TypeSafePropBagMetaData() { } // Disallow use of parameterless constructor.

        public TypeSafePropBagMetaData(string fullClassName, PropBagTypeSafetyMode typeSafetyMode, IPropFactory thePropFactory)
        {
            //ClassName = className ?? throw new ArgumentNullException(nameof(className));
            FullClassName = fullClassName ?? throw new ArgumentNullException(nameof(fullClassName));
            TypeSafetyMode = typeSafetyMode;
            ThePropFactory = thePropFactory ?? throw new ArgumentNullException(nameof(thePropFactory));

            SetPolicyProperties(typeSafetyMode, out bool allPropsMustBeRegistered, out bool onlyTypedAccess,
                out ReadMissingPropPolicyEnum readMissingPropPolicy,
                out bool returnDefaultForUndefined);

            AllPropsMustBeRegistered = allPropsMustBeRegistered;
            OnlyTypedAccess = onlyTypedAccess;
            ReadMissingPropPolicy = readMissingPropPolicy;
            ReturnDefaultForUndefined = returnDefaultForUndefined;
        }

        string _className;
        public string ClassName
        {
            get
            {
                if(_className == null)
                {
                    _className = GetClassNameFromFullName(FullClassName);
                }
                return _className;
            }
        }

        public string FullClassName { get; }

        public PropBagTypeSafetyMode TypeSafetyMode { get; protected set; }

        IPropFactory ThePropFactory { get; }

        /// <summary>
        /// If true, attempting to set a property for which no call to AddProp has been made, will cause an exception to thrown.
        /// </summary>
        public bool AllPropsMustBeRegistered { get; }

        /// <summary>
        /// If not true, attempting to set a property, not previously set with a call to AddProp or SetIt<typeparamref name="T"/>, will cause an exception to be thrown.
        /// </summary>
        public bool OnlyTypedAccess { get; }

        public ReadMissingPropPolicyEnum ReadMissingPropPolicy { get; }

        public bool ReturnDefaultForUndefined { get; }

        protected bool GetReturnDefaultForUndefined(PropBagTypeSafetyMode typeSafetyMode)
            => typeSafetyMode == PropBagTypeSafetyMode.None || typeSafetyMode == PropBagTypeSafetyMode.RegisterOnGetLoose;

        protected void SetPolicyProperties(PropBagTypeSafetyMode typeSafetyMode, out bool allPropsMustBeRegistered,
            out bool onlyTypedAccess, out ReadMissingPropPolicyEnum readMissingPropPolicy, out bool returnDefaultForUndefined)
        {
            returnDefaultForUndefined = GetReturnDefaultForUndefined(typeSafetyMode);

            switch (typeSafetyMode)
            {
                case PropBagTypeSafetyMode.Locked:
                    {
                        allPropsMustBeRegistered = true;
                        onlyTypedAccess = true;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.NotAllowed;
                        break;
                    }
                case PropBagTypeSafetyMode.Tight:
                    {
                        allPropsMustBeRegistered = true;
                        onlyTypedAccess = true;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        break;
                    }
                case PropBagTypeSafetyMode.AllPropsMustBeRegistered:
                    {
                        allPropsMustBeRegistered = true;
                        onlyTypedAccess = false;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        break;
                    }
                case PropBagTypeSafetyMode.OnlyTypedAccess:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = true;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        break;
                    }
                case PropBagTypeSafetyMode.HonorUndefined:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = false;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        break;
                    }
                case PropBagTypeSafetyMode.None:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = false;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        break;
                    }
                case PropBagTypeSafetyMode.RegisterOnGetLoose:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = false;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Register;
                        break;
                    }
                case PropBagTypeSafetyMode.RegisterOnGetSafe:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = true;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Register;
                        break;
                    }
                default:
                    throw new ApplicationException($"Unexpected value for the {nameof(typeSafetyMode)} parameter was found during the creation of a {nameof(TypeSafePropBagMetaData)}.");
            }

        }

        // In order to verify that a given class that implements IPropFactory has a value of 'ReturnDefaultForUndefined'
        // that is compatible with the specified TypeSafetyMode and
        // since it is true that instances of TypeSafePropBagMetaData calculate the value of 'ReturnDefaultForUndefined'
        // from a value of TypeSafetyMode,
        // and since the caller cannot (easily) create an instance of a TypeSafePropBagMetaData, 
        // this helper class (which can create empty instances of a TypeSafePropBagMetaData) 
        // is provided to fill this gap.

        // If someone derives a class from TypeSafePropBagMetaData, they should also provide a new class
        // to provide this service.
        public static class Helper
        {
            public static bool GetReturnDefaultForUndefined(PropBagTypeSafetyMode typeSafetyMode)
            {
                return new TypeSafePropBagMetaData().GetReturnDefaultForUndefined(typeSafetyMode);
            }
        }

        private string GetClassNameFromFullName(string fullName)
        {
            if (fullName == null)
            {
                return null;
            }

            IEnumerable<string> temp = fullName.Split('.');
            string lastTerm = temp.Last();

            return lastTerm;
        }
    }
}
