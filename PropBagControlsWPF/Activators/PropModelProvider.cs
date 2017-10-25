using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF.WPFHelpers;
using DRM.TypeSafePropertyBag;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBag.ControlsWPF
{
    public class PropModelProvider : IPropModelProvider
    {
        private IPropBagTemplateProvider _propBagTemplateProvider;

        public bool CanFindPropBagTemplateWithJustKey => _propBagTemplateProvider?.CanFindPropBagTemplateWithJustKey != false;
        public bool HasPbtLookupResources => _propBagTemplateProvider != null;

        #region Constructors

        public PropModelProvider()
        {
            _propBagTemplateProvider = null;
        }

        public PropModelProvider(IPropBagTemplateProvider propBagTemplateProvider) 
        {
            _propBagTemplateProvider = propBagTemplateProvider;
        }

        #endregion

        #region PropBagTemplate Locator Support

        public PropModel GetPropModel(string resourceKey)
        {
            try
            {
                if (CanFindPropBagTemplateWithJustKey)
                {
                    PropBagTemplate pbt = _propBagTemplateProvider.GetPropBagTemplate(resourceKey);
                    PropModel pm = GetPropModel(pbt);
                    return pm;
                }
                else if (_propBagTemplateProvider == null)
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a PropBagTemplateProvider upon construction. " +
                        $"No class implementing: {nameof(IPropBagTemplateProvider)} was provided. " +
                        $"Please supply a PropBagTemplate object.");
                }
                else
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with the necessary resources upon construction. " +
                        $"A {_propBagTemplateProvider.GetType()} was provided, but it does not have the necessary resources. " +
                        $"Please supply a ResourceDictionary and ResourceKey or a ProbBagTemplate object.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException("Resource was not found.", e);
            }
        }

        public PropModel GetPropModel(ResourceDictionary rd, string resourceKey)
        {
            try
            {
                if (_propBagTemplateProvider != null)
                {
                    PropBagTemplate pbt = _propBagTemplateProvider.GetPropBagTemplate(resourceKey);
                    PropModel pm = GetPropModel(pbt);
                    return pm;
                }
                else
                {
                    throw new InvalidOperationException($"A call providing a ResourceDictionary and a ResouceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a resource upon construction. " +
                        $"No class implementing: {nameof(IPropBagTemplateProvider)} was provided.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException("Resource was not found.", e);
            }
        }

        #endregion

        #region Parsing Logic

        public PropModel GetPropModel(PropBagTemplate pbt)
        {
            string className = pbt.ClassName;
            string instanceKey = pbt.InstanceKey;
            string outputNamespace = pbt.OutPutNameSpace;
            bool deriveFromPubPropBag = pbt.DeriveFromPubPropBag;
            PropBagTypeSafetyMode typeSafetyMode = pbt.TypeSafetyMode;
            bool deferMethodRefResolution = pbt.DeferMethodRefResolution;
            bool requireExplicitInitialValue = pbt.RequireExplicitInitialValue;

            PropModel result =
                new PropModel(className,
                    instanceKey,
                    outputNamespace,
                    deriveFromPubPropBag,
                    typeSafetyMode,
                    deferMethodRefResolution,
                    requireExplicitInitialValue);


            int namespacesCount = pbt.Namespaces == null ? 0 : pbt.Namespaces.Count;
            for (int nsPtr = 0; nsPtr < namespacesCount; nsPtr++)
            {
                result.Namespaces.Add(pbt.Namespaces[nsPtr].Namespace);
            }

            int propsCount = pbt.Props == null ? 0 : pbt.Props.Count;

            for (int propPtr = 0; propPtr < propsCount; propPtr++)
            {
                PropItem pi = pbt.Props[propPtr];

                bool hasStore = pi.HasStore;
                bool typeIsSolid = pi.TypeIsSolid;
                string extraInfo = pi.ExtraInfo;

                ControlModel.PropItem rpi = new ControlModel.PropItem(pi.PropertyType, pi.PropertyName,
                    hasStore, typeIsSolid, pi.PropKind, extraInfo: extraInfo);

                foreach (Control uc in pi.Items)
                {
                    // ToDo: Find and process this field first, and then enter the enclosing foreach loop.
                    // because one day, some of the other processing may depend on the PropertyType.
                    if (uc is PropTypeInfoField tif)
                    {
                        Type propertyType = GetTypeFromInfoField(tif, pi.PropKind, pi.PropertyType, out Type itemType);
                        if (pi.PropKind == PropKindEnum.Collection)
                        {
                            rpi.CollectionType = propertyType;
                            rpi.ItemType = itemType;
                        }
                        else
                        {
                            rpi.PropertyType = propertyType;
                            rpi.ItemType = null;
                        }
                    }

                    else if (uc is InitialValueField ivf)
                    {
                        ControlModel.PropInitialValueField rivf = new ControlModel.PropInitialValueField(
                            ivf.InitialValue, ivf.SetToDefault, ivf.SetToUndefined, ivf.SetToNull, ivf.SetToEmptyString);

                        rpi.InitialValueField = rivf;
                    }

                    else if (uc is PropDoWhenChangedField dwc)
                    {
                        ControlModel.PropDoWhenChangedField rdwc =
                            new ControlModel.PropDoWhenChangedField(dwc.DoWhenChangedAction.DoWhenChanged, dwc.DoAfterNotify);

                        rpi.DoWhenChangedField = rdwc;
                    }

                    else if (uc is PropComparerField pcf)
                    {
                        ControlModel.PropComparerField rpcf =
                            new ControlModel.PropComparerField(pcf.ComparerFunc.Comparer, pcf.UseRefEquality);

                        rpi.ComparerField = rpcf;
                    }
                }

                result.Props.Add(rpi);

            }
            return result;
        }

        private Type GetTypeFromInfoField(PropTypeInfoField tif, PropKindEnum propKind,
            Type propertyType, out Type itemType)
        {
            itemType = null;
            switch (propKind)
            {
                case PropKindEnum.Prop:
                    {
                        itemType = null;
                        return propertyType;
                    }
                case PropKindEnum.Collection:
                    {
                        return GetTypeFromCollInfoField(tif, out itemType);
                    }
                case PropKindEnum.DataTable:
                    {
                        itemType = null;
                        return typeof(DataTable);
                    }
                default:
                    {
                        throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
                    }
            }
        }

        private Type GetTypeFromCollInfoField(PropTypeInfoField tif, out Type itemType)
        {
            WellKnownCollectionTypeEnum collectionType = tif.CollectionType ?? WellKnownCollectionTypeEnum.Enumerable;

            switch (collectionType)
            {
                //case WellKnownCollectionTypeEnum.Enumerable:
                //    {
                //        return propertyType;
                //    }
                //case WellKnownCollectionTypeEnum.List:
                //    {
                //        break;
                //    }
                case WellKnownCollectionTypeEnum.ObservableCollection:
                    {
                        foreach (object uc in tif.Items)
                        {
                            if (uc is PropTypeInfoCollection ptiCollection)
                            {
                                if (ptiCollection.Count > 0)
                                {
                                    PropTypeInfoField fChild = ptiCollection[0];
                                    itemType = fChild.PropertyType;
                                    return GetObsCollType(itemType);
                                }
                            }

                            else if (uc is PropTypeInfoField child)
                            {
                                itemType = child.PropertyType;
                                return GetObsCollType(itemType);
                            }
                        }

                        throw new ArgumentException("Cannot find a PropTypeInfoField for this collection.");
                    }
                default:
                    {
                        throw new InvalidOperationException($"CollectionType = {collectionType} is not recognized or is not supported.");
                    }
            }
        }

        private Type GetObsCollType(Type itemType)
        {
            //Type typDestDType = typeof(ObservableCollection<PersonVM>);
            //string strDestDType = typDestDType.FullName;
            //string asqnTypeName = typDestDType.AssemblyQualifiedName;
            //Type tt = Type.GetType(asqnTypeName);
            //Type pt = typeof(PersonVM);
            //string asqnPTName = pt.AssemblyQualifiedName;

            string typeName = $"System.Collections.ObjectModel.ObservableCollection`1[[{itemType.AssemblyQualifiedName}]], System, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = b77a5c561934e089";
            Type result = Type.GetType(typeName);

            return result;
        }

        #endregion
    }
}
