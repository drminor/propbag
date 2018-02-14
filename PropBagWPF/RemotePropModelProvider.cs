using DRM.PropBag;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace DRM.PropBagWPF
{
    public class RemotePropModelProvider : IProvidePropModels
    {
        #region Private Fields

        private IParsePropBagTemplates _pbtParser;
        private ResourceDictionaryProvider _resourceDictionaryProvider;

        private IPropFactoryFactory _propFactoryFactory;
        private IPropFactory _defaultPropFactory;

        private Dictionary<string, IPropModel> _propModelCache;
        private Dictionary<string, IMapperRequest> _mapperRequestCache;

        #endregion

        #region Constructors

        public RemotePropModelProvider
            (
            ResourceDictionaryProvider resourceDictionaryProvider,
            IParsePropBagTemplates propBagTemplateParser,
            IPropFactoryFactory propFactoryFactory,
            IPropFactory defaultPropFactory
            )

        {
            _resourceDictionaryProvider = resourceDictionaryProvider;
            _pbtParser = propBagTemplateParser;
            _propFactoryFactory = propFactoryFactory;
            _defaultPropFactory = defaultPropFactory;

            _propModelCache = new Dictionary<string, IPropModel>();
            _mapperRequestCache = new Dictionary<string, IMapperRequest>();

            
        }

        #endregion

        #region Bulk Parsing

        public IDictionary<string, IPropModel> LoadPropModels(string basePath, string[] filenames)
        {
            Dictionary<string, IPropModel> result = new Dictionary<string, IPropModel>();

            Thread thread = new Thread(() =>
            {
                ResourceDictionary rd = _resourceDictionaryProvider.Load(basePath, filenames);

                foreach(ResourceDictionary rdChild in rd.MergedDictionaries)
                {
                    foreach(DictionaryEntry kvp in rdChild)
                    {
                        if(kvp.Value is PropBagTemplate pbt)
                        {
                            IPropModel propModel = _pbtParser.ParsePropModel(pbt);

                            result.Add((string) kvp.Key, propModel);
                            _propModelCache.Add((string)kvp.Key, propModel);
                        }
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            thread = null;

            _propModelCache = result;
            return result;
        }

        public IDictionary<string, IMapperRequest> LoadMapperRequests(string basePath, string[] filenames)
        {
            Dictionary<string, IMapperRequest> result = new Dictionary<string, IMapperRequest>();

            Thread thread = new Thread(() =>
            {
                ResourceDictionary rd = _resourceDictionaryProvider.Load(basePath, filenames);

                foreach (ResourceDictionary rdChild in rd.MergedDictionaries)
                {
                    foreach (DictionaryEntry kvp in rdChild)
                    {
                        if (kvp.Value is MapperRequestTemplate mr)
                        {
                            // Go ahead and fetch the PropModel from the key specified in the "template" request -- since the 
                            // the receiver of this PropBag.MapperRequest will probably not have access to a PropModel Provider.
                            //IPropModel propModel = GetPropModel(mr.DestinationPropModelKey);
                            //IMapperRequest mrCooked = new MapperRequest(mr.SourceType, propModel, mr.ConfigPackageName);

                            IMapperRequest mrCooked = new MapperRequest(mr.SourceType, mr.DestinationPropModelKey, mr.ConfigPackageName);

                            result.Add((string)kvp.Key, mrCooked);
                            _mapperRequestCache.Add((string)kvp.Key, mrCooked);
                        }
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            thread = null;

            _mapperRequestCache = result;
            return result;
        }

        #endregion

        #region PropBagTemplate Locator Support

        public bool CanFindPropBagTemplateWithJustKey => true;
        public bool HasPbtLookupResources => true;

        public IPropModel GetPropModel(string resourceKey)
        {
            if(_propModelCache == null)
            {
                throw new InvalidOperationException("You must first call LoadPropModels.");
            }

            if(_propModelCache.TryGetValue(resourceKey, out IPropModel propModel))
            {
                return FixUpPropFactory(propModel, _defaultPropFactory);
            }
            else
            {
                throw new KeyNotFoundException("No PropModel with that resource key can be found.");
            }
        }

        IPropModel FixUpPropFactory(IPropModel propModel, IPropFactory fallBackPropFactory)
        {
            // If the propModel does not specify a PropFactory, but it does specify a PropFactoryType,
            // use the PropFactoryFactory given to us to create a PropFactory.
            
            if(propModel.PropFactory == null)
            {
                if(propModel.PropFactoryType != null)
                {
                    IPropFactory created = _propFactoryFactory.BuildPropFactory(propModel.PropFactoryType);
                    propModel.PropFactory = created;
                }
                else
                {
                    // If no value was supplied for either the PropFactory or the PropFactoryType,
                    // then use the default or 'fallback' propFactory.
                    propModel.PropFactory = fallBackPropFactory;
                }
            }

            // If the propModel does not supply a PropFactory, use the one assigned to us upon construction.
            return propModel;
        }

        #endregion

        #region AutoMapperRequest Lookup Support

        public bool CanFindMapperRequestWithJustKey => true;
        public bool HasMrLookupResources => true;

        public IMapperRequest GetMapperRequest(string resourceKey)
        {
            if (_mapperRequestCache == null)
            {
                throw new InvalidOperationException("You must first call LoadMapperRequests.");
            }

            if (_mapperRequestCache.TryGetValue(resourceKey, out IMapperRequest mrCooked))
            {
                return mrCooked;
            }
            else
            {
                throw new KeyNotFoundException("No MapperRequest with that resource key can be found.");
            }
        }

        #endregion

        //#region Parsing Logic

        //private IPropModel GetPropModel(PropBagTemplate pbt, IPropFactory propFactory)
        //{
        //    if(pbt.ClassName == "PersonCollectionViewModel")
        //    {
        //        System.Diagnostics.Debug.WriteLine($"We are processing the {pbt.ClassName} PropItem.");
        //    }

        //    //SimpleExKey test = pbt.GetTestObject();

        //    DeriveFromClassModeEnum deriveFrom = pbt.DeriveFromClassMode;
        //    Type targetType = pbt.TargetType;

        //    //IAMServiceRef test = pbt.Au

        //    //TypeInfoField wrapperTypeInfoField = GetWrapperTypeInfo(pbt);
        //    //Type targetTypeFromWTInfoField = GetTypeFromInfoField(wrapperTypeInfoField, PropKindEnum.Prop, targetType, out Type itemTypeDummy);

        //    IPropFactory propFactoryToUse;
        //    //if (propFactory == null)
        //    //{
        //    //    IPropFactory fromTemplate = GetPropFactory(pbt);
        //    //    if(fromTemplate == null)
        //    //    {
        //    //        propFactoryToUse = _defaultPropFactory;
        //    //    }
        //    //    else
        //    //    {
        //    //        propFactoryToUse = fromTemplate;
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    propFactoryToUse = propFactory;
        //    //}

        //    //if (propFactoryToUse == null)
        //    //{
        //    //    throw new InvalidOperationException($"Could not get a value for the PropFactory when fetching the PropModel: {pbt.FullClassName}");
        //    //}

        //    propFactoryToUse = propFactory ?? GetPropFactory(pbt) ?? _defaultPropFactory ??
        //        throw new InvalidOperationException($"Could not get a value for the PropFactory when fetching the PropModel: {pbt.FullClassName}");

        //    IPropModel result = new PropModel
        //        (
        //        className: pbt.ClassName,
        //        namespaceName: pbt.OutPutNameSpace,
        //        deriveFrom: deriveFrom,
        //        targetType: targetType,
        //        propFactoryType: pbt.PropFactoryType,
        //        typeSafetyMode: pbt.TypeSafetyMode,
        //        deferMethodRefResolution: pbt.DeferMethodRefResolution,
        //        requireExplicitInitialValue: pbt.RequireExplicitInitialValue
        //        );

        //    int namespacesCount = pbt.Namespaces == null ? 0 : pbt.Namespaces.Count;
        //    for (int nsPtr = 0; nsPtr < namespacesCount; nsPtr++)
        //    {
        //        result.Namespaces.Add(pbt.Namespaces[nsPtr].Namespace);
        //    }

        //    DoWhenChangedHelper doWhenChangedHelper = new DoWhenChangedHelper();

        //    int propsCount = pbt.Props == null ? 0 : pbt.Props.Count;

        //    for (int propPtr = 0; propPtr < propsCount; propPtr++)
        //    {
        //        IPropTemplateItem pi = pbt.Props[propPtr];

        //        try
        //        {
        //            IPropItem rpi = ProcessProp(pi/*, propFactoryToUse*/, doWhenChangedHelper);
        //            result.Props.Add(rpi);
        //        }
        //        catch (Exception e)
        //        {
        //            throw new InvalidOperationException($"Error: {e.Message} occured while parsing PropItem: {pi.PropertyName} for class: {pbt.ClassName}.", e);
        //        }

        //    }
        //    return result;
        //}

        //private IPropFactory GetPropFactory(PropBagTemplate propBagTemplate)
        //{
        //    return null;
        //}

        //private IPropItem ProcessProp(IPropTemplateItem pi, DoWhenChangedHelper doWhenChangedHelper)
        //{
        //    PropStorageStrategyEnum storageStrategy = pi.StorageStrategy;
        //    bool typeIsSolid = pi.TypeIsSolid;
        //    string extraInfo = pi.ExtraInfo;

        //    IPropItem rpi = new PropItemModel(pi.PropertyType, pi.PropertyName,
        //        storageStrategy, typeIsSolid, pi.PropKind, extraInfo: extraInfo);

        //    bool isCProp = pi.PropKind.IsCollection(); 
        //    bool foundTypeInfoField = false;

        //    ItemCollection items = ((PropItem)pi).Items;

        //    foreach (Control uc in items)
        //    {
        //        // ToDo: Find and process this field first, and then enter the enclosing foreach loop.
        //        // because one day, some of the other processing may depend on the PropertyType.

        //        // Type Info Field
        //        if (uc is DRM.PropBagControlsWPF.TypeInfoField tif)
        //        {
        //            foundTypeInfoField = true;
        //            Type propertyType = GetTypeFromInfoField(tif, pi.PropKind, pi.PropertyType, out Type itemType);
        //            if (isCProp)
        //            {
        //                rpi.CollectionType = propertyType;
        //                rpi.ItemType = itemType;
        //            }
        //            else
        //            {
        //                rpi.PropertyType = propertyType;
        //                rpi.ItemType = null;
        //            }
        //        }

        //        // Initial Value Field
        //        else if (uc is InitialValueField ivf)
        //        {
        //            PropInitialValueField rivf;

        //            // TODO: Add error handling here.
        //            if (ivf.PropBagResourceKey != null)
        //            {
        //                //IPropModel pm = GetPropModel(ivf.PropBagResourceKey);

        //                //Func<object> vc = () => _viewModelActivator.GetNewViewModel(pm, _storeAccessCreator, pi.PropertyType, pm.PropFactory ?? _defaultPropFactory, pm.FullClassName);

        //                rivf = new PropInitialValueField(initialValue: null, setToDefault: false, setToUndefined: false,
        //                    setToEmptyString: false, setToNull: false, valueCreator: null, createNew: ivf.CreateNew, propBagResourceKey: ivf.PropBagResourceKey);
        //            }
        //            else if (ivf.CreateNew)
        //            {
        //                //Func<object> vc = () => Activator.CreateInstance(pi.PropertyType);
        //                rivf = new PropInitialValueField(initialValue: null, setToDefault: false, setToUndefined: false,
        //                    setToEmptyString: false, setToNull: false, valueCreator: null, createNew: ivf.CreateNew, propBagResourceKey: ivf.PropBagResourceKey);
        //            }
        //            else
        //            {
        //                rivf = new PropInitialValueField(ivf.InitialValue, ivf.SetToDefault, ivf.SetToUndefined,
        //                    ivf.SetToNull, ivf.SetToEmptyString, valueCreator: null, createNew: false, propBagResourceKey: null);
        //            }

        //            rpi.InitialValueField = rivf;

        //        }

        //        // Do When Changed Field
        //        else if (uc is DRM.PropBagControlsWPF.PropDoWhenChangedField dwc)
        //        {
        //            MethodInfo mi = doWhenChangedHelper.GetMethodAndSubKind(dwc, rpi.PropertyType, rpi.PropertyName, out SubscriptionKind subscriptionKind);

        //            SubscriptionPriorityGroup priorityGroup = dwc?.DoAfterNotify ?? false ? SubscriptionPriorityGroup.Last : SubscriptionPriorityGroup.Standard;

        //            IPropDoWhenChangedField rdwc = new DRM.PropBag.PropDoWhenChangedField
        //                (
        //                target: null, method: mi,
        //                subscriptionKind: subscriptionKind, priorityGroup: priorityGroup,
        //                methodIsLocal: true, declaringType: null,
        //                fullClassName: null, instanceKey: null
        //                );

        //            rpi.DoWhenChangedField = rdwc;
        //        }

        //        // Comparer Field
        //        else if (uc is DRM.PropBagControlsWPF.PropComparerField pcf)
        //        {
        //            PropBag.PropComparerField rpcf =
        //                new PropBag.PropComparerField(pcf.ComparerFunc.Comparer, pcf.UseRefEquality);

        //            rpi.ComparerField = rpcf;
        //        }

        //        // Local Binder Field
        //        else if (uc is DRM.PropBagControlsWPF.PropBinderField binderField)
        //        {
        //            PropBag.PropBinderField rBinderField =
        //                new PropBag.PropBinderField(binderField.Path);

        //            rpi.BinderField = rBinderField;
        //            rpi.MapperRequestResourceKey = binderField.MapperRequestResourceKey;

        //            //if (binderField.MapperRequestResourceKey != null)
        //            //{
        //            //    IMapperRequest mr = GetMapperRequest(binderField.MapperRequestResourceKey);
        //            //    rpi.MapperRequest = mr;
        //            //}
        //        }
        //    }

        //    if(!foundTypeInfoField && isCProp)
        //    {
        //        Type propertyType = GetTypeFromInfoField(null, pi.PropKind, pi.PropertyType, out Type itemType);
        //        rpi.CollectionType = propertyType;
        //        rpi.ItemType = itemType;
        //    }

        //    return rpi;
        //}

        //private DRM.PropBagControlsWPF.TypeInfoField GetWrapperTypeInfo(PropBagTemplate pbt)
        //{
        //    DRM.PropBagControlsWPF.TypeInfoField ptif = pbt.Items.OfType<DRM.PropBagControlsWPF.TypeInfoField>().FirstOrDefault();
        //    return ptif;
        //}

        //private Type GetTypeFromInfoField(DRM.PropBagControlsWPF.TypeInfoField tif, PropKindEnum propKind,
        //    Type propertyType, out Type itemType)
        //{
        //    itemType = null;
        //    switch (propKind)
        //    {
        //        case PropKindEnum.Prop:
        //            itemType = null;
        //            return propertyType;

        //        case PropKindEnum.CollectionView:
        //            itemType = propertyType;
        //            Type pType = typeof(ICollectionView);
        //            return pType;

        //        case PropKindEnum.Enumerable_RO:
        //            goto case PropKindEnum.ObservableCollection;
        //        case PropKindEnum.Enumerable:
        //            goto case PropKindEnum.ObservableCollection;

        //        case PropKindEnum.EnumerableTyped_RO:
        //            goto case PropKindEnum.ObservableCollection;
        //        case PropKindEnum.EnumerableTyped:
        //            goto case PropKindEnum.ObservableCollection;


        //        case PropKindEnum.ObservableCollection_RO:
        //            goto case PropKindEnum.ObservableCollection;
        //        case PropKindEnum.ObservableCollection:
        //            return GetTypeFromCollInfoField(tif, propertyType, out itemType);


        //        case PropKindEnum.CollectionViewSource_RO:
        //            goto case PropKindEnum.CollectionViewSource;

        //        case PropKindEnum.CollectionViewSource:
        //            Type result = typeof(CollectionViewSource);
        //            itemType = propertyType;
        //            return result;


        //        case PropKindEnum.DataTable_RO:
        //            goto case PropKindEnum.DataTable;

        //        case PropKindEnum.DataTable:
        //            itemType = null;
        //            return typeof(DataTable);

        //        default:
        //            throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
        //    }
        //}

        //private Type GetTypeFromCollInfoField(DRM.PropBagControlsWPF.TypeInfoField tif, Type propertyType, out Type itemType)
        //{
        //    WellKnownCollectionTypeEnum collectionType = tif?.CollectionType ?? WellKnownCollectionTypeEnum.ObservableCollectionFB;

        //    switch (collectionType)
        //    {
        //        case WellKnownCollectionTypeEnum.ObservableCollectionFB:
        //            {
        //                Type cType;
        //                if (tif == null)
        //                {
        //                    itemType = propertyType;
        //                    cType = GetObservableCollType(itemType);
        //                }
        //                else
        //                {
        //                    cType = GetObsCollectionType(tif, out itemType, GetObservableCollType);
        //                }
        //                return cType;
        //            }
        //        //case WellKnownCollectionTypeEnum.ObservableCollection:
        //        //    {
        //        //        Type cType;
        //        //        if (tif == null)
        //        //        {
        //        //            itemType = propertyType;
        //        //            cType = GetObsCollType(itemType);
        //        //        }
        //        //        else
        //        //        {
        //        //            cType = GetObsCollectionType(tif, out itemType, GetObsCollType);
        //        //        }

        //        //        return cType;
        //        //    }
        //        case WellKnownCollectionTypeEnum.Enumerable:
        //            {
        //                goto case WellKnownCollectionTypeEnum.ObservableCollectionFB;
        //            }
        //        default:
        //            {
        //                throw new InvalidOperationException($"CollectionType = {collectionType} is not recognized or is not supported.");
        //            }
        //    }
        //}

        //private Type GetObsCollectionType(DRM.PropBagControlsWPF.TypeInfoField tif, out Type itemType, Func<Type, Type> typeGetter)
        //{
        //    // Consider using the MemberType property of the "root" TypeInfoField itself.
        //    TypeInfoCollection tps;
        //    DRM.PropBagControlsWPF.TypeInfoField child;

        //    // Use our type parameter arguments 1-3
        //    if (tif.TypeParameter1 != null)
        //    {
        //        itemType = tif.TypeParameter1;
        //        return GetObservableCollType(itemType);
        //    }

        //    // Use our child Type Parameters
        //    else if (tif.TypeParameters != null && tif.TypeParameters.Count > 0)
        //    {
        //        child = tif.TypeParameters[0];
        //        itemType = child.MemberType;
        //        return typeGetter(itemType); // GetObservableCollType(itemType);
        //    }

        //    // Use a child control of type TypeInfoCollection
        //    else if (null != (tps = tif.Items.OfType<TypeInfoCollection>().FirstOrDefault()))
        //    {
        //        if (tps.Count > 0)
        //        {
        //            child = tps[0];
        //            itemType = child.MemberType;
        //            return GetObservableCollType(itemType);
        //        }
        //    }

        //    // Use a child control of type TypeInfoField
        //    else if (null != (child = tif.Items.OfType<DRM.PropBagControlsWPF.TypeInfoField>().FirstOrDefault()))
        //    {
        //        itemType = child.MemberType;
        //        return GetObservableCollType(itemType);
        //    }

        //    throw new ArgumentException("Cannot find a PropTypeInfoField for this collection.");
        //}

        //// TODO: Consider using Expression.GetDelegateType here instead of using hard coded Assembly names.
        //private Type GetObservableCollType(Type itemType)
        //{
        //    //Type typDestDType = typeof(ObservableCollection<PersonVM>);
        //    //string strDestDType = typDestDType.FullName;
        //    //string asqnTypeName = typDestDType.AssemblyQualifiedName;
        //    //Type tt = Type.GetType(asqnTypeName);
        //    //Type pt = typeof(PersonVM);
        //    //string asqnPTName = pt.AssemblyQualifiedName;

        //    string typeName = $"System.Collections.ObjectModel.ObservableCollection`1[[{itemType.AssemblyQualifiedName}]], System, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = b77a5c561934e089";
        //    Type result = Type.GetType(typeName);

        //    return result;
        //}

        //#endregion
    }
}
