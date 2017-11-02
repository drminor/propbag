
using System.Reflection;
using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;


namespace PropBagLib.Tests
{
	public partial class ExtStoreModel : PropBag
	{
		//public ExtStoreModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered) { }

		public ExtStoreModel(PropBagTypeSafetyMode typeSafetyMode, IPropFactory factory) : base(typeSafetyMode, null, factory)
		{
	        AddProp<int>("PropInt3", null, false, null);
	        AddProp<int>("PropInt4", null, false, null);
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
		  
			public event PropertyChangedWithTValsHandler<int> PropInt3Changed
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
	  
			public event PropertyChangedWithTValsHandler<int> PropInt4Changed
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
		private Action<T, T> GetDelegate<T>(string methodName)
		{
		    Type pp = this.GetType();
		    MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		
		    if (mi == null) return null;
		
		    Action<T, T> result = (Action<T, T>)mi.CreateDelegate(typeof(Action<T, T>), this);
		
		    return result;
		}


	} 
}
