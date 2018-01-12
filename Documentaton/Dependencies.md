# Service Dependencies

All interfaces are defined in DRM.TypeSafePropertyBag, unless otherwise noted.

## PropBag Dependencies
IPropBag => PropModel (This is a class with no defined interface, defined in namespace: DRM.PropBag.ControlModel.)

PropModel => IPropFactory

IPropFactory =>
- ResolveTypeDelegate,
- IConvertValues
- IProvideDelegateCaches

IProvidePropStoreAccessService<L2T, L2TRaw> =>
- int maxPropsPerObject,
- IProvideHandlerDispatchDelegateCaches
 
IConvertValues => TypeDescBasedTConverterCache (This is a class with no defined interface, defined in namespace: DRM.TypeSafePropertyBag.)


## PropStoreAccessService Dependencies
IPropStoreAccessService<L2T, L2TRaw> =>
- StoreNodeBag,   (Class with no defined interface, defined in namespace: DRM.TypeSafePropertyBag.)
- IL2KeyMan<L2T, L2TRaw>, 
- IProvidePropStoreAccessService<L2T, L2TRaw>,
- IProvideHandlerDispatchDelegateCaches

StoreNodeBag =>
- IPropBagProxy, 
- int ObjectId,  (New ObjectIds are provided by thread-safe sequential number generator, internal to each IProvidePropStoreAccessService implementation.)
- ICacheDelegates&lt;CallPSParentNodeChangedEventSubDelegate&gt;

IPropBagProxy => WeakReference&lt;IPropBagInternal&gt;

IPropBagInternal => IPropStoreAccessService<L2T, L2TRaw>

Note: All IPropBag implementations also implement IPropBagInternal. These implementation rely on the PropFactory to 
create a class that implements: IPropStoreAccessService.

## ViewModel Instantiation Services

DataContextProvider => (This is a class with no defined interface, defined in namespace: MVVMApplication.Infra.)
- ViewModelHelper (This is a class with no defined interface, defined in namespace: DRM.PropBagWPF.)

ViewModelHelper => 
- IPropModelProvider (Defined in namespace: DRM.PropBag.ControlModel)
- IViewModelActivator (Defined in namespace: DRM.ViewModelTools.)
- IPropStoreAccessServiceCreator<L2T, L2TRaw> (Implemented by all IProvidePropStoreAccessService<L2T, L2TRaw>)

IPropModelProvider =>
- IPropBagTemplateProvider
- IPropFactory fallBackPropFactory (Will propably be removed and then all PropModel instances must specify a IPropFactory.)
- IViewModelActivator viewModelActivator
- IPropStoreAccessServiceCreator<L2T, L2TRaw>

IViewModelActivator => none

IPropBagTemplateProvider => ResourceDictionary (Optional)












