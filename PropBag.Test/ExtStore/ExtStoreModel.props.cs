
using System;
using System.Collections.Generic;
using PropBagLib.Tests;
using System.Reflection;
using DRM.PropBag;
using DRM.Ipnwvc;


namespace PropBagLib.Tests
{
	public partial class ExtStoreModel : PropBag
	{
		public ExtStoreModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

		public ExtStoreModel(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

		public ExtStoreModel(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory factory) : base(typeSafetyMode, factory)
		{
	        AddProp<object>("PropObject", null, false, null);
	        AddProp<string>("PropString", null, false, null);
		}

	#region Property Declarations
		  
		public object PropObject
		{
			get
			{
				return GetIt<object>();
			}
			set
			{
				SetIt<object>(value);
			}
		}  
	  
		public string PropString
		{
			get
			{
				return GetIt<string>();
			}
			set
			{
				SetIt<string>(value);
			}
		}  
	 
	#endregion

	#region PropetyChangedWithTVals Event Declarations
		  
			public event PropertyChangedWithTValsHandler<object> PropObjectChanged
			{
				add
				{
					AddToPropChanged<object>(value);
				}
				remove
				{
					RemoveFromPropChanged<object>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<string> PropStringChanged
			{
				add
				{
					AddToPropChanged<string>(value);
				}
				remove
				{
					RemoveFromPropChanged<string>(value);
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
