using System;

namespace DRM.TypeSafePropertyBag
{
    public class TypeSafePropBagMetaData
    {
        private TypeSafePropBagMetaData() { } // Disallow use of parameterless constructor.

        public TypeSafePropBagMetaData(string className, string classFullName, PropBagTypeSafetyMode typeSafetyMode, IPropFactory thePropFactory)
        {
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
            ClassFullName = classFullName ?? throw new ArgumentNullException(nameof(classFullName));
            TypeSafetyMode = typeSafetyMode;
            ThePropFactory = thePropFactory ?? throw new ArgumentNullException(nameof(thePropFactory));

            SetPolicyProperties(typeSafetyMode, out bool allPropsMustBeRegistered, out bool onlyTypedAccess,
                out ReadMissingPropPolicyEnum readMissingPropPolicy,
                out bool returnDefaultForUndefined);

            AllPropsMustBeRegistered = AllPropsMustBeRegistered;
            OnlyTypedAccess = onlyTypedAccess;
            ReadMissingPropPolicy = readMissingPropPolicy;
            ReturnDefaultForUndefined = returnDefaultForUndefined;
        }

        public string ClassName { get; }

        public string ClassFullName { get; }

        public PropBagTypeSafetyMode TypeSafetyMode { get; protected set; }

        IPropFactory ThePropFactory { get; }

        /// <summary>
        /// If true, attempting to set a property for which no call to AddProp has been made, will cause an exception to thrown.
        /// </summary>
        public virtual bool AllPropsMustBeRegistered { get; }

        /// <summary>
        /// If not true, attempting to set a property, not previously set with a call to AddProp or SetIt<typeparamref name="T"/>, will cause an exception to be thrown.
        /// </summary>
        public virtual bool OnlyTypedAccess { get; }

        public virtual ReadMissingPropPolicyEnum ReadMissingPropPolicy { get; }

        public virtual bool ReturnDefaultForUndefined { get; }

        protected void SetPolicyProperties(PropBagTypeSafetyMode typeSafetyMode, out bool allPropsMustBeRegistered,
            out bool onlyTypedAccess, out ReadMissingPropPolicyEnum readMissingPropPolicy, out bool returnDefaultForUndefined)
        {
            switch (typeSafetyMode)
            {
                case PropBagTypeSafetyMode.Locked:
                    {
                        allPropsMustBeRegistered = true;
                        onlyTypedAccess = true;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.NotAllowed;
                        returnDefaultForUndefined = false;
                        break;
                    }
                case PropBagTypeSafetyMode.Tight:
                    {
                        allPropsMustBeRegistered = true;
                        onlyTypedAccess = true;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        returnDefaultForUndefined = false;
                        break;
                    }
                case PropBagTypeSafetyMode.AllPropsMustBeRegistered:
                    {
                        allPropsMustBeRegistered = true;
                        onlyTypedAccess = false;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        returnDefaultForUndefined = false;
                        break;
                    }
                case PropBagTypeSafetyMode.OnlyTypedAccess:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = true;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        returnDefaultForUndefined = false;
                        break;
                    }
                case PropBagTypeSafetyMode.HonorUndefined:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = false;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        returnDefaultForUndefined = false;
                        break;
                    }
                case PropBagTypeSafetyMode.None:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = false;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Allowed;
                        returnDefaultForUndefined = true;
                        break;
                    }
                case PropBagTypeSafetyMode.RegisterOnGetLoose:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = false;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Register;
                        returnDefaultForUndefined = true;
                        break;
                    }
                case PropBagTypeSafetyMode.RegisterOnGetSafe:
                    {
                        allPropsMustBeRegistered = false;
                        onlyTypedAccess = true;
                        readMissingPropPolicy = ReadMissingPropPolicyEnum.Register;
                        returnDefaultForUndefined = false;
                        break;
                    }
                default:
                    throw new ApplicationException("Unexpected value for typeSafetyMode parameter.");

            }

        }
    }
}
