# Top-Level Service Dependencies

IPropBag => PropModel

PropModel => IPropFactory

IPropFactory => IProvidePropStoreAccessService<UInt32, string>, ResolveTypeDelegate, IConvertValues

IProvidePropStoreAccessService<L2T, L2TRaw> => int maxPropsPerObject, IProvideHandlerDispatchDelegateCaches
 
IConvertValues =>  TypeDescBasedTConverterCache (This is a class with no defined interface.)

-----
IPropStoreAccessService<L2T, L2TRaw> => StoreNodeBag, IL2KeyMan<L2T, L2TRaw>, 
        IProvidePropStoreAccessService<L2T, L2TRaw>, IProvideHandlerDispatchDelegateCaches

StoreNodeBag => IPropBag, (new Object Id), DelegateCache<CallPSParentNodeChangedEventSubDelegate>

-----




## Place Holder.
