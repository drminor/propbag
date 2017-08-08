
using System.Reflection;
using DRM.PropBag;
using DRM.Ipnwvc;
using System;
using System.Collections.Generic;
using PropBagLib.Tests;


namespace PropBagLib.Tests
{
	public partial class AllPropsRegisteredModel : PubPropBag
	{
		public AllPropsRegisteredModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

		public AllPropsRegisteredModel(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

		public AllPropsRegisteredModel(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory factory) : base(typeSafetyMode, factory)
		{
	        AddProp<object>("PropObject", null, false, null);
	        AddProp<string>("PropString", GetDelegate<string>("DoWhenStringChanged"), false, null);
	        AddPropNoValue<string>("PropStringCallDoAfter", GetDelegate<string>("DoWhenStringChanged"), true, EqualityComparer<string>.Default);
	        AddPropObjComp<string>("PropStringUseRefComp", GetDelegate<string>("DoWhenStringChanged"), true, null);
	        AddProp<bool>("PropBool", null, false, null);
	        AddProp<int>("PropInt", null, false, null);
	        AddProp<TimeSpan>("PropTimeSpan", null, false, null);
	        AddProp<Uri>("PropUri", null, false, null);
	        AddProp<Lazy<int>>("PropLazyInt", null, false, null);
	        AddProp<Nullable<int>>("PropNullableInt", GetDelegate<Nullable<int>>("DoWhenNullIntChanged"), false, null, null, -1);
	        AddProp<ICollection<int>>("PropICollectionInt", GetDelegate<ICollection<int>>("DoWhenICollectionIntChanged"), false, null);
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
	  
		public string PropStringCallDoAfter
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
	  
		public string PropStringUseRefComp
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
	  
		public bool PropBool
		{
			get
			{
				return GetIt<bool>();
			}
			set
			{
				SetIt<bool>(value);
			}
		}  
	  
		public int PropInt
		{
			get
			{
				return GetIt<int>();
			}
			set
			{
				SetIt<int>(value);
			}
		}  
	  
		public TimeSpan PropTimeSpan
		{
			get
			{
				return GetIt<TimeSpan>();
			}
			set
			{
				SetIt<TimeSpan>(value);
			}
		}  
	  
		public Uri PropUri
		{
			get
			{
				return GetIt<Uri>();
			}
			set
			{
				SetIt<Uri>(value);
			}
		}  
	  
		public Lazy<int> PropLazyInt
		{
			get
			{
				return GetIt<Lazy<int>>();
			}
			set
			{
				SetIt<Lazy<int>>(value);
			}
		}  
	  
		public Nullable<int> PropNullableInt
		{
			get
			{
				return GetIt<Nullable<int>>();
			}
			set
			{
				SetIt<Nullable<int>>(value);
			}
		}  
	  
		public ICollection<int> PropICollectionInt
		{
			get
			{
				return GetIt<ICollection<int>>();
			}
			set
			{
				SetIt<ICollection<int>>(value);
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
	  
			public event PropertyChangedWithTValsHandler<string> PropStringCallDoAfterChanged
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
	  
			public event PropertyChangedWithTValsHandler<string> PropStringUseRefCompChanged
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
	  
			public event PropertyChangedWithTValsHandler<bool> PropBoolChanged
			{
				add
				{
					AddToPropChanged<bool>(value);
				}
				remove
				{
					RemoveFromPropChanged<bool>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<int> PropIntChanged
			{
				add
				{
					AddToPropChanged<int>(value);
				}
				remove
				{
					RemoveFromPropChanged<int>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<TimeSpan> PropTimeSpanChanged
			{
				add
				{
					AddToPropChanged<TimeSpan>(value);
				}
				remove
				{
					RemoveFromPropChanged<TimeSpan>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<Uri> PropUriChanged
			{
				add
				{
					AddToPropChanged<Uri>(value);
				}
				remove
				{
					RemoveFromPropChanged<Uri>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<Lazy<int>> PropLazyIntChanged
			{
				add
				{
					AddToPropChanged<Lazy<int>>(value);
				}
				remove
				{
					RemoveFromPropChanged<Lazy<int>>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<Nullable<int>> PropNullableIntChanged
			{
				add
				{
					AddToPropChanged<Nullable<int>>(value);
				}
				remove
				{
					RemoveFromPropChanged<Nullable<int>>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<ICollection<int>> PropICollectionIntChanged
			{
				add
				{
					AddToPropChanged<ICollection<int>>(value);
				}
				remove
				{
					RemoveFromPropChanged<ICollection<int>>(value);
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
