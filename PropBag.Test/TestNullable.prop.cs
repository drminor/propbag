
using DRM.Ipnwv;
using DRM.PropBag;

public partial class TestNullable : PropBag
{
	public TestNullable() : this(PropBagTypeSafetyMode.Loose) { }

	public TestNullable(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode)
	{
		AddProp<int>("Prop1");
		AddProp<string>("Prop2");
		AddProp<double>("Prop3");
		
	}

#region Property Declarations
	  
	public int Prop1
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
  
	public string Prop2
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
  
	public double Prop3
	{
		get
		{
			return GetIt<double>();
		}
		set
		{
			SetIt<double>(value);
		}
	}  
 
#endregion

#region PropetyChangedWithTVals Event Declarations
	  
        public event PropertyChangedWithTValsHandler<int> Prop1Changed
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
  
        public event PropertyChangedWithTValsHandler<string> Prop2Changed
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
  
        public event PropertyChangedWithTValsHandler<double> Prop3Changed
        {
            add
            {
                AddToPropChanged<double>(value);
            }
            remove
            {
                RemoveFromPropChanged<double>(value);
            }
        }
 
#endregion

} 
