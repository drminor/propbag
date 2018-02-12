﻿using DRM.PropBag;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;

using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class SimplePropModelProvider : IProvidePropModels, IDisposable
    { 
        #region Private Fields

        private IPropBagTemplateProvider _propBagTemplateProvider;

        private IMapperRequestProvider _mapperRequestProvider;

        private IViewModelActivator _viewModelActivator;
        private PSAccessServiceCreatorInterface _storeAccessCreator;
        private IPropFactory _fallBackPropFactory;

        #endregion

        #region Constructors

        public SimplePropModelProvider(
            IPropBagTemplateProvider propBagTemplateProvider,
            IMapperRequestProvider mapperRequestProvider,
            IPropFactory fallBackPropFactory,
            IViewModelActivator viewModelActivator,
            PSAccessServiceCreatorInterface storeAccessCreator) 
        {
            _propBagTemplateProvider = propBagTemplateProvider;
            _mapperRequestProvider = mapperRequestProvider;
            _viewModelActivator = viewModelActivator;
            _storeAccessCreator = storeAccessCreator;
            _fallBackPropFactory = fallBackPropFactory;
        }

        #endregion

        #region PropBagTemplate Locator Support

        public bool CanFindPropBagTemplateWithJustKey => _propBagTemplateProvider?.CanFindPropBagTemplateWithJustAKey != false;
        public bool HasPbtLookupResources => _propBagTemplateProvider != null;

        public IPropModel GetPropModel(string resourceKey)
        {
            return GetPropModel(resourceKey, null);
        }

        public IPropModel GetPropModel(string resourceKey, IPropFactory propFactory)
        {
            try
            {
                if (CanFindPropBagTemplateWithJustKey)
                {
                    PropBagTemplate pbt = _propBagTemplateProvider.GetPropBagTemplate(resourceKey);
                    IPropModel pm = GetPropModel(pbt, propFactory);
                    return pm;
                }
                else if (HasPbtLookupResources)
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
                throw new ApplicationException($"PropBagTemplate for ResourceKey = {resourceKey} was not found.", e);
            }
        }

        public IPropModel GetPropModel(ResourceDictionary rd, string resourceKey)
        {
            return GetPropModel(rd, resourceKey, null);
        }

        public IPropModel GetPropModel(ResourceDictionary rd, string resourceKey, IPropFactory propFactory)
        {
            try
            {
                if (HasPbtLookupResources)
                {
                    PropBagTemplate pbt = _propBagTemplateProvider.GetPropBagTemplate(resourceKey);
                    IPropModel pm = GetPropModel(pbt, propFactory);
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

        #region AutoMapperRequest Lookup Support

        public bool CanFindMapperRequestWithJustKey => _mapperRequestProvider?.CanFindMapperRequestWithJustAKey != false;
        public bool HasMrLookupResources => _mapperRequestProvider != null;

        public IMapperRequest GetMapperRequest(string resourceKey)
        {
            try
            {
                if (CanFindMapperRequestWithJustKey)
                {
                    MapperRequestTemplate mr = _mapperRequestProvider.GetMapperRequest(resourceKey);

                    // Go ahead and fetch the PropModel from the key specified in the "template" request -- since the 
                    // the receiver of this PropBag.MapperRequest will probably not have access to a PropModel Provider.
                    IPropModel propModel = GetPropModel(mr.DestinationPropModelKey);

                    IMapperRequest mrCooked = new MapperRequest(mr.SourceType, propModel, mr.ConfigPackageName);
                    return mrCooked;
                }
                else if (HasMrLookupResources)
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a MapperRequestProvider upon construction. " +
                        $"No class implementing: {nameof(IMapperRequestProvider)} was provided. " +
                        $"Please supply a MapperRequest object.");
                }
                else
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with the necessary resources upon construction. " +
                        $"A {_mapperRequestProvider.GetType()} was provided, but it does not have the necessary resources. " +
                        $"Please supply a ResourceDictionary and ResourceKey or a MapperRequest object.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException($"MapperRequest for ResourceKey = {resourceKey} was not found.", e);
            }
        }

        public IMapperRequest GetMapperRequest(ResourceDictionary rd, string resourceKey)
        {
            try
            {
                if (HasMrLookupResources)
                {
                    MapperRequestTemplate mr = _mapperRequestProvider.GetMapperRequest(rd, resourceKey);

                    // Go ahead and fetch the PropModel from the key specified in the "template" request -- since the 
                    // the receiver of this PropBag.MapperRequest will probably not have access to a PropModel Provider.
                    IPropModel propModel = GetPropModel(mr.DestinationPropModelKey);

                    IMapperRequest mrCooked = new MapperRequest(mr.SourceType, propModel, mr.ConfigPackageName);
                    return mrCooked;
                }
                else
                {
                    throw new InvalidOperationException($"A call providing a ResourceDictionary and a ResouceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a resource upon construction. " +
                        $"No class implementing: {nameof(IMapperRequestProvider)} was provided.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException("Resource was not found.", e);
            }
        }

        #endregion

        #region Parsing Logic

        private IPropModel GetPropModel(PropBagTemplate pbt, IPropFactory propFactory)
        {
            if(pbt.ClassName == "PersonCollectionViewModel")
            {
                System.Diagnostics.Debug.WriteLine($"We are processing the {pbt.ClassName} PropItem.");
            }

            DeriveFromClassModeEnum deriveFrom = pbt.DeriveFromClassMode;
            Type targetType = pbt.TargetType;

            //TypeInfoField wrapperTypeInfoField = GetWrapperTypeInfo(pbt);
            //Type targetTypeFromWTInfoField = GetTypeFromInfoField(wrapperTypeInfoField, PropKindEnum.Prop, targetType, out Type itemTypeDummy);

            IPropFactory propFactoryToUse = propFactory ?? pbt.PropFactory ?? _fallBackPropFactory ??
                throw new InvalidOperationException($"Could not get a value for the PropFactory when fetching the PropModel: {pbt.FullClassName}");

            IPropModel result = new PropModel
                (
                className: pbt.ClassName,
                namespaceName: pbt.OutPutNameSpace,
                deriveFrom: deriveFrom,
                targetType: targetType,
                propStoreServiceProviderType: pbt.PropStoreServiceProviderType,
                propFactory: propFactoryToUse,
                typeSafetyMode: pbt.TypeSafetyMode,
                deferMethodRefResolution: pbt.DeferMethodRefResolution,
                requireExplicitInitialValue: pbt.RequireExplicitInitialValue
                );

            int namespacesCount = pbt.Namespaces == null ? 0 : pbt.Namespaces.Count;
            for (int nsPtr = 0; nsPtr < namespacesCount; nsPtr++)
            {
                result.Namespaces.Add(pbt.Namespaces[nsPtr].Namespace);
            }

            DoWhenChangedHelper doWhenChangedHelper = new DoWhenChangedHelper();

            int propsCount = pbt.Props == null ? 0 : pbt.Props.Count;

            for (int propPtr = 0; propPtr < propsCount; propPtr++)
            {
                PropItem pi = pbt.Props[propPtr];

                try
                {
                    IPropItem rpi = ProcessProp(pi, propFactoryToUse, doWhenChangedHelper);
                    result.Props.Add(rpi);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Error: {e.Message} occured while parsing PropItem: {pi.Name} for class: {pbt.ClassName}.", e);
                }

            }
            return result;
        }

        private IPropItem ProcessProp(PropItem pi, IPropFactory propFactory, DoWhenChangedHelper doWhenChangedHelper)
        {
            PropStorageStrategyEnum storageStrategy = pi.StorageStrategy;
            bool typeIsSolid = pi.TypeIsSolid;
            string extraInfo = pi.ExtraInfo;

            IPropItem rpi = new PropItemModel(pi.PropertyType, pi.PropertyName,
                storageStrategy, typeIsSolid, pi.PropKind, extraInfo: extraInfo);

            bool isCProp = pi.PropKind.IsCollection(); // propFactory.IsCollection(pi.PropKind);
            bool foundTypeInfoField = false;

            foreach (Control uc in pi.Items)
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
                    PropInitialValueField rivf;

                    // TODO: Add error handling here.
                    if (ivf.PropBagResourceKey != null)
                    {
                        IPropModel pm = GetPropModel(ivf.PropBagResourceKey);

                        Func<object> vc = () => _viewModelActivator.GetNewViewModel(pm, _storeAccessCreator, pi.PropertyType, pm.PropFactory ?? _fallBackPropFactory, pm.FullClassName);

                        rivf = new PropInitialValueField(initialValue: null, setToDefault: false, setToUndefined: false,
                            setToEmptyString: false, setToNull: false, valueCreator: vc);
                    }
                    else if (ivf.CreateNew)
                    {
                        Func<object> vc = () => Activator.CreateInstance(pi.PropertyType);
                        rivf = new PropInitialValueField(initialValue: null, setToDefault: false, setToUndefined: false,
                            setToEmptyString: false, setToNull: false, valueCreator: vc);
                    }
                    else
                    {
                        rivf = new PropInitialValueField(ivf.InitialValue, ivf.SetToDefault, ivf.SetToUndefined,
                            ivf.SetToNull, ivf.SetToEmptyString, valueCreator: null);
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
                    PropBag.PropComparerField rpcf =
                        new PropBag.PropComparerField(pcf.ComparerFunc.Comparer, pcf.UseRefEquality);

                    rpi.ComparerField = rpcf;
                }

                // Local Binder Field
                else if (uc is DRM.PropBagControlsWPF.PropBinderField binderField)
                {
                    PropBag.PropBinderField rBinderField =
                        new PropBag.PropBinderField(binderField.Path);

                    rpi.BinderField = rBinderField;

                    if(binderField.MapperRequestResourceKey != null)
                    {
                        IMapperRequest mr = GetMapperRequest(binderField.MapperRequestResourceKey);
                        rpi.MapperRequest = mr;
                    }
                }
            }

            if(!foundTypeInfoField && isCProp)
            {
                Type propertyType = GetTypeFromInfoField(null, pi.PropKind, pi.PropertyType, out Type itemType);
                rpi.CollectionType = propertyType;
                rpi.ItemType = itemType;
            }

            return rpi;
        }

        private DRM.PropBagControlsWPF.TypeInfoField GetWrapperTypeInfo(PropBagTemplate pbt)
        {
            DRM.PropBagControlsWPF.TypeInfoField ptif = pbt.Items.OfType<DRM.PropBagControlsWPF.TypeInfoField>().FirstOrDefault();
            return ptif;
        }

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
                    Type pType = typeof(ICollectionView);
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
            WellKnownCollectionTypeEnum collectionType = tif?.CollectionType ?? WellKnownCollectionTypeEnum.ObservableCollectionFB;

            switch (collectionType)
            {
                case WellKnownCollectionTypeEnum.ObservableCollectionFB:
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
                //case WellKnownCollectionTypeEnum.ObservableCollection:
                //    {
                //        Type cType;
                //        if (tif == null)
                //        {
                //            itemType = propertyType;
                //            cType = GetObsCollType(itemType);
                //        }
                //        else
                //        {
                //            cType = GetObsCollectionType(tif, out itemType, GetObsCollType);
                //        }

                //        return cType;
                //    }
                case WellKnownCollectionTypeEnum.Enumerable:
                    {
                        goto case WellKnownCollectionTypeEnum.ObservableCollectionFB;
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

        //private Type GetObsCollType(Type itemType)
        //{
        //    string assName = "TypeSafePropertyBag";

        //    //string assGUID = "bbb1e311 - 374e-4c91 - a313 - 1667d83767db";
        //    string typeName = $"DRM.TypeSafePropertyBag.IObsCollection`1[[{itemType.AssemblyQualifiedName}]], {assName}";
        //    Type result = Type.GetType(typeName);

        //    return result;
        //}

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

    }
}