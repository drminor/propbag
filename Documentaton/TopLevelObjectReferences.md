# Top-Level Object References

The Store has...
	A List of StoreNodeBags, keyed by Global PropId (Dictionary<ExKey, StoreNodeBag>)

Each StoreNodeBag has...
	A WeakReference to the IPropBag (Via PropBagProxy)
	A StoreNodeProp Parent if hosted by another PropBag's PropItem.
	Has a list of child StoreNodeProps


Each IPropBag has...
	Has a StoreAccessor (via IPropBagInternal)
	A PropFactory

Each StoreAccessor has...
	A WeakReference to the PropBag it serves.
	A List of subscribed event handlers, one for each child PropItem. The references to the subscribers are weak.
	A list of local binders.

Each Local Binder has...
	A list of PropertyChange event subscriptions. (Only weak references to the event sources are kept.)
	A weak reference to the PropBag that holds the target property.
	A strong reference to the StoreAccessor for the PropBag that holds the target property.

	
Each PropFactory has...
	A reference to the Global Store via the StoreAccessServiceProvider











	
	