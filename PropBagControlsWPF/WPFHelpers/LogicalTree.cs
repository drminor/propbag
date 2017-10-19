using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.WPFHelpers
{
    public class LogicalTree
    {
        #region Get DataContext from FrameworkElement

        // If this is being used to create a binding to set the DataContext
        // Then:
        //      Start with the node's parent.
        // Else:
        //      Start with this node.
        public static DependencyObject GetDataContext(DependencyObject depObj, 
            out bool foundIt,
            bool startWithParent = false, bool inspectAncestors = true, 
            bool stopOnNodeWithBoundDc = true)
        {
            //if (stopOnNodeWithBoundDc && !inspectAncestors) throw new InvalidOperationException("Can't stop on NodeWithBoundDc, if inspectAncestors is false.");

            bool hasDataContext = false;
            DependencyObject curObj;
            DependencyObject foundNode;
            bool dataContextPropIsBound;
            bool shouldStopDueToDcIsPropBound = false;

            if (startWithParent)
            {
                curObj = LogicalTreeHelper.GetParent(depObj);
            }
            else
            {
                curObj = depObj;
            }

            do
            {
                foundNode = curObj;

                if (curObj is FrameworkElement fe)
                {
                    hasDataContext = HasDataContext(fe, out curObj, out dataContextPropIsBound);
                }
                else if(curObj is FrameworkContentElement fce)
                {
                    hasDataContext = HasDataContext(fce, out curObj, out dataContextPropIsBound);
                }
                else
                {
                    // I beleive that every node returned by Fe.Parent / Fce.Parent is a FrameworkElement or a FrameworkContentElement.
                    // but just in case...
                    curObj = LogicalTreeHelper.GetParent(curObj);
                    dataContextPropIsBound = false;
                    hasDataContext = false;
                }

                shouldStopDueToDcIsPropBound = stopOnNodeWithBoundDc && dataContextPropIsBound;

            }
            while (!hasDataContext && curObj != null && !shouldStopDueToDcIsPropBound && inspectAncestors);

            if(object.ReferenceEquals(foundNode, depObj))
            {
                System.Diagnostics.Debug.WriteLine("Logical Tree did not have to traverse any ancestors.");
            }
            foundIt = hasDataContext;
            return foundNode;
        }

        public static DependencyObject GetDataContextWithBoundDc(DependencyObject depObj,
            out bool foundIt,
                bool startWithParent = false)
        {
            bool dataContextPropIsBound;
            DependencyObject curObj;
            DependencyObject foundNode;

            if (startWithParent)
            {
                curObj = LogicalTreeHelper.GetParent(depObj);
            }
            else
            {
                curObj = depObj;
            }

            do
            {
                foundNode = curObj;

                if (curObj is FrameworkElement fe)
                {
                    bool dummy = HasDataContext(fe, out curObj, out dataContextPropIsBound);
                }
                else if (curObj is FrameworkContentElement fce)
                {
                    bool dummy = HasDataContext(fce, out curObj, out dataContextPropIsBound);
                }
                else
                {
                    // I beleive that every node returned by Fe.Parent / Fce.Parent is a FrameworkElement or a FrameworkContentElement.
                    // but just in case...
                    curObj = LogicalTreeHelper.GetParent(curObj);
                    dataContextPropIsBound = false;
                }

            }
            while (!dataContextPropIsBound && curObj != null);

            if (object.ReferenceEquals(foundNode, depObj))
            {
                System.Diagnostics.Debug.WriteLine("Logical Tree did not have to traverse any ancestors.");
            }
            foundIt = dataContextPropIsBound;
            return foundNode;
        }


        public static bool HasDataContext(DependencyObject depObj, 
            out DependencyObject parent, out bool dataContextPropIsBound)
        {
            if (depObj is FrameworkElement fe)
            {
                parent = fe.Parent;

                dataContextPropIsBound = fe.GetBindingExpression(FeDataContextDpPropProvider.Value) != null;
                return fe.DataContext != null; 
            }
            else if (depObj is FrameworkContentElement fce)
            {
                parent = fce.Parent;
                dataContextPropIsBound = fce.GetBindingExpression(FceDataContextDpPropProvider.Value) != null;
                return fce.DataContext != null; 
            }
            else
            {
                dataContextPropIsBound = false;
                parent = LogicalTreeHelper.GetParent(depObj);
                return false;
            }
        }

        public static bool HasDcBinding(DependencyObject depObj)
        {
            if (depObj is FrameworkElement fe)
            {
                BindingExpression be = fe.GetBindingExpression(FeDataContextDpPropProvider.Value);
                return be != null;
            }
            else if (depObj is FrameworkContentElement fce)
            {
                BindingExpression be = fce.GetBindingExpression(FceDataContextDpPropProvider.Value);
                return be != null;
            }
            else
            {
                throw new InvalidOperationException("DepObj must be a FrameworkElement or a FrameworkContentElement.");
            }
        }

        public static bool GetDcFromFrameworkElement(object feOrFce, out object dc, out Type type)
        {
            type = null;
            if (feOrFce is FrameworkElement fe)
            {
                dc = fe.DataContext;
                if (dc != null)
                {
                    type = dc.GetType();
                }
                return true;
            }
            else if (feOrFce is FrameworkContentElement fce)
            {
                dc = fce.DataContext;
                if (dc != null)
                {
                    type = dc.GetType();
                }
                return true;
            }
            else
            {
                dc = null;
                return false;
            }
        }


        #endregion

        #region Dependency Property Support

        public static Lazy<DependencyProperty> FeDataContextDpPropProvider =
            new Lazy<DependencyProperty>(
                () => GetDependencyPropertyByName(typeof(FrameworkElement), "DataContextProperty"),
                LazyThreadSafetyMode.PublicationOnly);

        public static Lazy<DependencyProperty> FceDataContextDpPropProvider =
            new Lazy<DependencyProperty>(
                () => GetDependencyPropertyByName(typeof(FrameworkContentElement), "DataContextProperty"),
                LazyThreadSafetyMode.PublicationOnly);

        public static Lazy<DependencyProperty> DataGridColumn_DisplayIndex_DpPropProvider =
            new Lazy<DependencyProperty>(
                () => GetDependencyPropertyByName(typeof(DataGridColumn), "DisplayIndexProperty"),
                LazyThreadSafetyMode.PublicationOnly);

        public static DependencyProperty GetDependencyPropertyByName(DependencyObject depObj, string dpName)
        {
            return GetDependencyPropertyByName(depObj.GetType(), dpName);
        }

        public static DependencyProperty GetDependencyPropertyByName(Type depObj, string dpName)
        {
            DependencyProperty dp = null;

            //FieldInfo[] allFs = depObj.GetFields();

            var fieldInfo = depObj.GetField(dpName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (fieldInfo != null)
            {
                dp = fieldInfo.GetValue(null) as DependencyProperty;
            }

            return dp;
        }
        #endregion


        #region CLR Property Support

        private static Type Data_Grid_Column_Type = typeof(DataGridColumn);

        public static PropertyInfo Data_Grid_ColumnOwner_Property_Info = Data_Grid_Column_Type.GetProperty("DataGridOwner", BindingFlags.Instance | BindingFlags.NonPublic);

        public static PropertyInfo Data_Grid_Column_Binding_Property_Info = Data_Grid_Column_Type.GetProperty("BindingProperty", BindingFlags.Instance | BindingFlags.Public);


        #endregion

        #region Miscellaneous

        public static string GetNameFromDepObject(DependencyObject depObj)
        {
            if (depObj is FrameworkElement fe)
            {
                return fe.Name;
            }
            else if (depObj is FrameworkContentElement fce)
            {
                return $"(fce:) {fce.Name}";
            }
            else
            {
                return $"{depObj.ToString()}";
            }
        }

        // TODO: Move these to the region above: Dependency Property Support


        public static DataGrid GetDataGridOwner(DataGridColumn dgc)
        {

            PropertyInfo[] testPropertyList = Data_Grid_Column_Type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
            object attempt;

            try
            {
                attempt = Data_Grid_ColumnOwner_Property_Info.GetValue(dgc, null);
            }
            catch
            {
                return null;
            }

            if(attempt == null)
            {
                return null;
            }
            else if(attempt is DataGrid dg)
            {
                return dg;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
