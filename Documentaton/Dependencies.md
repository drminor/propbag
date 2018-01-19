# Service Dependencies

All interfaces are defined in DRM.TypeSafePropertyBag, unless otherwise noted.

## PropBag Dependencies
IPropBag / IPropBagInternal =>
- PropModel (This is a class with no defined interface, defined in namespace: DRM.PropBag.ControlModel.)
- IPropStoreAccessService<UInt32, String>

Notes:

- [ ] All IPropBag implementations also implement IPropBagInternal.
- [ ] IPropBag could easily be changed to IPropBag<L2T, L2TRaw>, and we would use a Type Alias such as  ``` using PropBagInterface = IPropBag<UInt32, String>; ```

PropModel =>
- IPropFactory

IPropFactory =>
- IProvideDelegateCaches
- IConvertValues
- ResolveTypeDelegate (Func<string, Type>)


IProvidePropStoreAccessService<L2T, L2TRaw> =>
- int maxPropsPerObject,
- IProvideHandlerDispatchDelegateCaches
 
IConvertValues =>
- ITypeDescBasedTConverterCache


## PropStoreAccessService Dependencies
IPropStoreAccessService<L2T, L2TRaw> =>
- StoreNodeBag,   (Class with no defined interface, defined in namespace: DRM.TypeSafePropertyBag.)
- IL2KeyMan<L2T, L2TRaw>, 
- IProvidePropStoreAccessService<L2T, L2TRaw>,
- IProvideHandlerDispatchDelegateCaches

StoreNodeBag =>
- IPropBagProxy, 
- int ObjectId,  (New ObjectIds are provided by thread-safe sequential number generator, internal to each IProvidePropStoreAccessService implementation.)
- IProvideDelegateCaches (Currently: ICacheDelegates&lt;CallPSParentNodeChangedEventSubDelegate&gt; but will be changed soon.)

IPropBagProxy =>
- WeakReference&lt;IPropBagInternal&gt;


## ViewModel Instantiation Services

DataContextProvider => (This is a class with no defined interface, defined in namespace: MVVMApplication.Infra.)
- ViewModelHelper (This is a class with no defined interface, defined in namespace: DRM.PropBagWPF.)

ViewModelHelper => 
- IPropModelProvider (Defined in namespace: DRM.PropBag.ControlModel)
- IViewModelActivator (Defined in namespace: DRM.PropBag.ViewModelTools.)
- IPropStoreAccessServiceCreator<L2T, L2TRaw> (Implemented by all IProvidePropStoreAccessService<L2T, L2TRaw>)

IPropModelProvider =>
- IPropBagTemplateProvider
- IPropFactory fallBackPropFactory (Will propably be removed and then all PropModel instances must specify a IPropFactory.)
- IViewModelActivator viewModelActivator
- IPropStoreAccessServiceCreator<L2T, L2TRaw>

IViewModelActivator => none

IPropBagTemplateProvider => ResourceDictionary (Optional)












