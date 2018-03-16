using DRM.PropBag;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public class PropBagTemplateParser : IParsePropBagTemplates
    {
        #region Constructors

        public PropBagTemplateParser()
        {
        }

        #endregion

        #region Parse Model

        public PropModelType ParsePropModel(PropBagTemplate pbt)
        {
            if (pbt.ClassName == "PersonCollectionViewModel")
            {
                System.Diagnostics.Debug.WriteLine($"We are processing the {nameof(pbt.ClassName)} PropItem.");
            }

            //DeriveFromClassModeEnum deriveFrom = pbt.DeriveFromClassMode;
            //Type targetType = pbt.TargetType;

            //TypeInfoField wrapperTypeInfoField = GetWrapperTypeInfo(pbt);
            //Type targetTypeFromWTInfoField = GetTypeFromInfoField(wrapperTypeInfoField, PropKindEnum.Prop, targetType, out Type itemTypeDummy);

            PropModelType result = new PropModel
                (
                className: pbt.ClassName,
                namespaceName: pbt.OutPutNameSpace,
                deriveFrom: pbt.DeriveFromClassMode,
                targetType: pbt.TargetType,
                propFactory: null,
                propFactoryType: pbt.PropFactoryType,
                propModelCache: null,
                typeSafetyMode: pbt.TypeSafetyMode,
                deferMethodRefResolution: pbt.DeferMethodRefResolution,
                requireExplicitInitialValue: pbt.RequireExplicitInitialValue
                );

            // Add Namespaces
            int namespacesCount = pbt.Namespaces == null ? 0 : pbt.Namespaces.Count;
            for (int nsPtr = 0; nsPtr < namespacesCount; nsPtr++)
            {
                result.Namespaces.Add(pbt.Namespaces[nsPtr].Namespace);
            }

            DoWhenChangedHelper doWhenChangedHelper = new DoWhenChangedHelper();

            int propsCount = pbt.Props == null ? 0 : pbt.Props.Count;

            // Parse each Prop Item
            for (int propPtr = 0; propPtr < propsCount; propPtr++)
            {
                IPropTemplateItem pi = pbt.Props[propPtr];

                try
                {
                    IPropItemModel rpi = ProcessProp(pi, doWhenChangedHelper);
                    result.Add(rpi.PropertyName, rpi);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Error: {e.Message} occured while parsing PropItem: {pi.PropertyName} for class: {pbt.ClassName}.", e);
                }

            }

            return result;
        }

        #endregion

        #region Parse Item

        private IPropItemModel ProcessProp(IPropTemplateItem pi, DoWhenChangedHelper doWhenChangedHelper)
        {
            PropStorageStrategyEnum storageStrategy = pi.StorageStrategy;
            bool typeIsSolid = pi.TypeIsSolid;
            string extraInfo = pi.ExtraInfo;

            IPropItemModel rpi = new PropItemModel
                (
                type: pi.PropertyType,
                name: pi.PropertyName,
                storageStrategy: storageStrategy,
                typeIsSolid: typeIsSolid,
                propKind: pi.PropKind,
                propTypeInfoField: null,
                initialValueField: null,
                extraInfo: extraInfo,
                comparer: null,
                itemType: null,
                binderField: null,
                mapperRequest: null,
                propCreator: null 
                );

            bool isCProp = pi.PropKind.IsCollection();
            bool foundTypeInfoField = false;

            ItemCollection items = ((PropItem)pi).Items;

            foreach (Control uc in items)
            {
                // ToDo: Find and process this field first, and then enter the enclosing foreach loop.
                // because one day, some of the other processing may depend on the PropertyType.

                // Type Info Field
                if (uc is DRM.PropBagControlsWPF.TypeInfoField tif)
                {
                    foundTypeInfoField = true;
                    Type propertyType = GetTypeFromInfoField(tif, pi.PropKind, pi.PropertyType, out Type itemType);
                    if (isCProp)
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

                // Initial Value Field
                else if (uc is InitialValueField ivf)
                {
                    IPropInitialValueField rivf;

                    // TODO: Add error handling here.
                    if (ivf.PropBagFullClassName != null)
                    {
                        rivf = PropInitialValueField.FromPropBagFCN(ivf.PropBagFullClassName);
                    }
                    else if (ivf.CreateNew)
                    {
                        rivf = PropInitialValueField.UseCreateNew;
                    }
                    else
                    {
                        rivf = new PropInitialValueField(ivf.InitialValue, ivf.SetToDefault, ivf.SetToUndefined,
                            ivf.SetToNull, ivf.SetToEmptyString);
                    }

                    rpi.InitialValueField = rivf;
                }

                // Do When Changed Field
                else if (uc is DRM.PropBagControlsWPF.PropDoWhenChangedField dwc)
                {
                    MethodInfo mi = doWhenChangedHelper.GetMethodAndSubKind(dwc, rpi.PropertyType, rpi.PropertyName, out SubscriptionKind subscriptionKind);

                    SubscriptionPriorityGroup priorityGroup = dwc?.DoAfterNotify ?? false ? SubscriptionPriorityGroup.Last : SubscriptionPriorityGroup.Standard;

                    IPropDoWhenChangedField rdwc = new DRM.PropBag.PropDoWhenChangedField
                        (
                        target: null, method: mi,
                        subscriptionKind: subscriptionKind, priorityGroup: priorityGroup,
                        methodIsLocal: true, declaringType: null,
                        fullClassName: null, instanceKey: null
                        );

                    rpi.DoWhenChangedField = rdwc;
                }

                // Comparer Field
                else if (uc is DRM.PropBagControlsWPF.PropComparerField pcf)
                {
                    IPropComparerField rpcf = new PropBag.PropComparerField(pcf.ComparerFunc.Comparer, pcf.UseRefEquality);

                    rpi.ComparerField = rpcf;
                }

                // Local Binder Field
                else if (uc is DRM.PropBagControlsWPF.PropBinderField binderField)
                {
                    IPropBinderField rBinderField = new PropBag.PropBinderField(binderField.Path);

                    rpi.BinderField = rBinderField;
                    rpi.MapperRequestResourceKey = binderField.MapperRequestResourceKey;
                }
            }

            if (!foundTypeInfoField)
            {
                if(isCProp)
                {
                    Type propertyType = GetTypeFromInfoField(null, pi.PropKind, pi.PropertyType, out Type itemType);
                    rpi.CollectionType = propertyType;
                    rpi.ItemType = itemType;
                }
                else if(pi.PropKind == PropKindEnum.CollectionView)
                {
                    rpi.PropertyType = typeof(ListCollectionView);
                    rpi.CollectionType = rpi.PropertyType;
                    rpi.ItemType = pi.PropertyType;
                }
                else
                {
                    // TODO: Check other PropKinds.
                    // Do Nothing.
                }
            }

            return rpi;
        }

        private DRM.PropBagControlsWPF.TypeInfoField GetWrapperTypeInfo(PropBagTemplate pbt)
        {
            DRM.PropBagControlsWPF.TypeInfoField ptif = pbt.Items.OfType<DRM.PropBagControlsWPF.TypeInfoField>().FirstOrDefault();
            return ptif;
        }

        #endregion

        #region Type Support

        private Type GetTypeFromInfoField(DRM.PropBagControlsWPF.TypeInfoField tif, PropKindEnum propKind,
            Type propertyType, out Type itemType)
        {
            itemType = null;
            switch (propKind)
            {
                case PropKindEnum.Prop:
                    itemType = null;
                    return propertyType;

                case PropKindEnum.CollectionView:
                    itemType = propertyType;
                    Type pType = typeof(ListCollectionView); // typeof(ICollectionView);
                    return pType;

                case PropKindEnum.Enumerable_RO:
                    goto case PropKindEnum.ObservableCollection;
                case PropKindEnum.Enumerable:
                    goto case PropKindEnum.ObservableCollection;

                case PropKindEnum.EnumerableTyped_RO:
                    goto case PropKindEnum.ObservableCollection;
                case PropKindEnum.EnumerableTyped:
                    goto case PropKindEnum.ObservableCollection;


                case PropKindEnum.ObservableCollection_RO:
                    goto case PropKindEnum.ObservableCollection;
                case PropKindEnum.ObservableCollection:
                    return GetTypeFromCollInfoField(tif, propertyType, out itemType);


                case PropKindEnum.CollectionViewSource_RO:
                    goto case PropKindEnum.CollectionViewSource;

                case PropKindEnum.CollectionViewSource:
                    Type result = typeof(CollectionViewSource);
                    itemType = propertyType;
                    return result;


                case PropKindEnum.DataTable_RO:
                    goto case PropKindEnum.DataTable;

                case PropKindEnum.DataTable:
                    itemType = null;
                    return typeof(DataTable);

                default:
                    throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        private Type GetTypeFromCollInfoField(DRM.PropBagControlsWPF.TypeInfoField tif, Type propertyType, out Type itemType)
        {
            WellKnownCollectionTypeEnum collectionType = tif?.CollectionType ?? WellKnownCollectionTypeEnum.ObservableCollection;

            switch (collectionType)
            {
                case WellKnownCollectionTypeEnum.ObservableCollection:
                    {
                        Type cType;
                        if (tif == null)
                        {
                            itemType = propertyType;
                            cType = GetObservableCollType(itemType);
                        }
                        else
                        {
                            cType = GetObsCollectionType(tif, out itemType, GetObservableCollType);
                        }
                        return cType;
                    }

                // Not Yet Supported
                //case WellKnownCollectionTypeEnum.TypedEnumerable:
                //    {
                //        break;
                //    }

                case WellKnownCollectionTypeEnum.Enumerable:
                    {
                        goto case WellKnownCollectionTypeEnum.ObservableCollection;
                    }
                default:
                    {
                        throw new InvalidOperationException($"CollectionType = {collectionType} is not recognized or is not supported.");
                    }
            }
        }

        private Type GetObsCollectionType(DRM.PropBagControlsWPF.TypeInfoField tif, out Type itemType, Func<Type, Type> typeGetter)
        {
            // Consider using the MemberType property of the "root" TypeInfoField itself.
            TypeInfoCollection tps;
            DRM.PropBagControlsWPF.TypeInfoField child;

            // Use our type parameter arguments 1-3
            if (tif.TypeParameter1 != null)
            {
                itemType = tif.TypeParameter1;
                return GetObservableCollType(itemType);
            }

            // Use our child Type Parameters
            else if (tif.TypeParameters != null && tif.TypeParameters.Count > 0)
            {
                child = tif.TypeParameters[0];
                itemType = child.MemberType;
                return typeGetter(itemType); // GetObservableCollType(itemType);
            }

            // Use a child control of type TypeInfoCollection
            else if (null != (tps = tif.Items.OfType<TypeInfoCollection>().FirstOrDefault()))
            {
                if (tps.Count > 0)
                {
                    child = tps[0];
                    itemType = child.MemberType;
                    return GetObservableCollType(itemType);
                }
            }

            // Use a child control of type TypeInfoField
            else if (null != (child = tif.Items.OfType<DRM.PropBagControlsWPF.TypeInfoField>().FirstOrDefault()))
            {
                itemType = child.MemberType;
                return GetObservableCollType(itemType);
            }

            throw new ArgumentException("Cannot find a PropTypeInfoField for this collection.");
        }

        // TODO: Consider using Expression.GetDelegateType here instead of using hard coded Assembly names.
        private Type GetObservableCollType(Type itemType)
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
