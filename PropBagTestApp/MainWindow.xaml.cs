using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections;

using System.Reflection; // This is temporary -- just for testing.

using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DRM.PropBag.ControlsWPF.PropBagInstanceAttribute("MainViewModel")]
        MainViewModel ourData;

        public bool PropFirstDidChange;
        public bool PropMyStringDidChange;
        
        public MainWindow()
        {
            PropFirstDidChange = false;
            InitializeComponent();

            DRM.PropBag.ControlsWPF.PropBagTemplate theModel = this.MainViewModelPropTemplate;
            DRM.PropBag.ControlModel.PropModel resultModel = theModel.GetPropBagModel();
            ourData = new MainViewModel(resultModel);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //string s = TestActionToString();

            //bool result = TestStringToAction(s);


            //PBControls.PropBagTemplate theModel = this.MainViewModelPropTemplate;

            //ControlModel.PropModel resultModel = PBControls.PropBagTemplate.GetModelFromControls();


            MessageBox.Show("Hello, we're leaveing.");
            this.Close();
        }

        private void DoWhenFirstChanges(bool oldVal, bool newVal)
        {
            this.PropFirstDidChange = true;
        }

        private void DoWhenMyStringChanges(string oldVal, string newVal)
        {
            this.PropMyStringDidChange = true;
        }

        //private ControlModel.PropModel GetModelFromControls(PBControls.PropBagTemplate theModel)
        //{
        //    string className = theModel.ClassName;
        //    string outputNamespace = theModel.OutPutNameSpace;

        //    ControlModel.PropModel result =
        //        new ControlModel.PropModel(className,
        //            outputNamespace,
        //            false,
        //            DRM.PropBag.PropBagTypeSafetyMode.AllPropsMustBeRegistered,
        //            true,
        //            true);


        //    int namespacesCount = theModel.Namespaces == null ? 0 : theModel.Namespaces.Count();
        //    for (int nsPtr = 0; nsPtr < namespacesCount; nsPtr++)
        //    {
        //        result.Namespaces.Add(theModel.Namespaces[nsPtr].Namespace);
        //    }

        //    int propsCount = theModel.Props == null ? 0 : theModel.Props.Count();

        //    for (int propPtr = 0; propPtr < propsCount; propPtr++)
        //    {
        //        PBControls.PropItem pi = theModel.Props[propPtr];

        //        bool hasStore = pi.HasStore; //.HasValue ? pi.HasStore.Value : true; // The default is true.
        //        bool typeIsSolid = pi.TypeIsSolid; //.HasValue ? pi.TypeIsSolid.Value : true; // The default is true.
        //        string extraInfo = pi.ExtraInfo; // ?? null;

        //        ControlModel.PropItem rpi = new ControlModel.PropItem(pi.PropertyType, pi.PropertyName, extraInfo, hasStore, typeIsSolid);

        //        foreach (Control uc in pi.Items)
        //        {
        //            if (uc is PBControls.InitialValueField)
        //            {
        //                PBControls.InitialValueField ivf = (PBControls.InitialValueField)uc;
        //                rpi.InitialValueField = new ControlModel.PropInitialValueField(ivf.InitialValue, ivf.SetToDefault, ivf.SetToUndefined, ivf.SetToNull, ivf.SetToEmptyString);
        //                continue;
        //            }

        //            if (uc is PBControls.PropDoWhenChangedField)
        //            {
        //                PBControls.PropDoWhenChangedField dwc = (PBControls.PropDoWhenChangedField)uc;

        //                //ControlModel.DoWhenChangedAction rdwcAction
        //                //    //= new ControlModel.DoWhenChangedAction(dwc.DoWhenChangedAction.ActionType, dwc.DoWhenChangedAction.ActionDelegate);
        //                //    = new ControlModel.DoWhenChangedAction(dwc.DoWhenChangedAction.ActionDelegate);

        //                ControlModel.PropDoWhenChangedField rdwc = new ControlModel.PropDoWhenChangedField(dwc.DoWhenChangedAction.ActionDelegate, dwc.DoAfterNotify);
                        
        //                rpi.DoWhenChangedField = rdwc; // new ControlModel.PropDoWhenChangedField(dwc.DoWhenChangedAction, dwc.DoAfterNotify);
        //                continue;
        //            }
        //        }

        //        result.Props.Add(rpi);

        //    }
        //    return result;
        //}


        #region Do When Changed Type Converter Tests

        private string TestActionToString()
        {
            Action<bool, bool> act = DoWhenFirstChanges;
            return GetStringFromAction(act);
        }

        private string GetStringFromAction(object value)
        {
            if (value is Delegate)
            {
                MethodInfo mi = ((Delegate)value).GetMethodInfo();

                string pt = mi.GetParameters()[0].ParameterType.FullName.ToString();

                return string.Format("{0}|{1}|{2}", pt, mi.DeclaringType, mi.Name);
            }
            else
            {
                return "Not Found";
            }
        }

        private bool TestStringToAction(string s)
        {
            string[] parts = s.Split('|');

            if (parts.Length < 3) return false;

            string propType = parts[0];
            string ownerType = parts[1];
            string methodName = parts[2];

            Type propTypeType = Type.GetType(propType);

            GetActionRefDelegate ActionGetter = GetTheGetActionRefDelegate(propTypeType);

            Delegate d = ActionGetter(ownerType, methodName, this);

            //Delegate d = GetActionFromString<bool>(ownerType, methodName);

            Delegate r = (Action<bool, bool>)DoWhenFirstChanges;

            return d == r;
        }

        #endregion

        #region Helper Methods for the Generic Method Templates

        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        // Delegate declarations.
        private delegate Delegate GetActionRefDelegate(string ownerType, string methodName, object target);

        private static GetActionRefDelegate GetTheGetActionRefDelegate(Type propertyType)
        {
            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetActionRefDelegate", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(propertyType);
            GetActionRefDelegate result = (GetActionRefDelegate)Delegate.CreateDelegate(typeof(GetActionRefDelegate), methInfoGetProp);

            return result;
        }

        #endregion
    
    } // End MainWindow

    #region Generic Method Templates

    static class GenericMethodTemplates
    {
        private static Delegate GetActionRefDelegate<T>(string ownerType, string methodName, object target)
        {
            Type ot = Type.GetType(ownerType);

            MethodInfo mi = ot.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (!IsDuoAction<T>(mi)) return null;

            //MainWindow mw = (MainWindow)target;

            Action<T,T> del = (Action<T,T>) Delegate.CreateDelegate(typeof(Action<T, T>), target, mi);

            return del;
        }


        static private bool IsDuoAction<T>(MethodInfo mi)
        {
            if (mi.ReturnType != typeof(void))
            {
                // Must return void.
                return false;
            }

            ParameterInfo[] parms = mi.GetParameters();

            if ((parms.Length != 2) || (parms[0].ParameterType != typeof(T)) || (parms[1].ParameterType != typeof(T)))
            {
                // Must have two parameters of the specified type.
                return false;
            }
            return true;
        }
    }

    #endregion
}
