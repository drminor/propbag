
using System.Reflection;
using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;


namespace PropBagLib.Tests
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class ExtStoreModel : PropBag
	{
		//public ExtStoreModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

		//public ExtStoreModel(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

		public ExtStoreModel(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator,
            string fullClassName, IPropFactory propFactory)
            : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName)
        {
	        AddProp<int>("PropInt3", comparer:null);
		 
	        AddProp<int>("PropInt4", comparer:null);
		 
		}

	#region Property Declarations
		  
		public int PropInt3
		{
			get
			{
				return GetIt<int>(nameof(PropInt3));
			}
			set
			{
				SetIt<int>(value, nameof(PropInt3));
			}
		}  
	  
		public int PropInt4
		{
			get
			{
				return GetIt<int>(nameof(PropInt4));
			}
			set
			{
				SetIt<int>(value, nameof(PropInt4));
			}
		}  
	 
	#endregion
	
	#region PropetyChangedWithTVals Event Declarations
		  
			public event EventHandler<PcTypedEventArgs<int>> PropInt3Changed
			{
				add
				{
					AddToPropChanged<int>(value, nameof(PropInt3Changed));
				}
				remove
				{
					RemoveFromPropChanged<int>(value, nameof(PropInt3Changed));
				}
			}
	  
			public event EventHandler<PcTypedEventArgs<int>> PropInt4Changed
			{
				add
				{
					AddToPropChanged<int>(value, nameof(PropInt4Changed));
				}
				remove
				{
					RemoveFromPropChanged<int>(value, nameof(PropInt4Changed));
				}
			}
	 
	#endregion

		/// <summary>
		/// If the delegate exists, the original name is returned,
		/// otherwise null is returned.
		/// </summary>
		/// <param name="methodName">Some public or non-public instance method in this class.</param>
		/// <returns>The name, unchanged, if the method exists, otherwise null.</returns>
		EventHandler<PcTypedEventArgs<T>> GetDelegate<T>(string methodName)
		{
		    Type pp = this.GetType();
		    MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		
		    if (mi == null) return null;
		
		    EventHandler<PcTypedEventArgs<T>> result = (EventHandler<PcTypedEventArgs<T>>)mi.CreateDelegate(typeof(EventHandler<PcTypedEventArgs<T>>), this);
		
		    return result;
		}


	} 
}
