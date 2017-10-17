using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.WPFHelpers
{
    public class LogicalTree
    {
        #region Get DataContext from FrameworkElement

        // TODO: What may be needed here is
        // If this is being used to create a binding to set the DataContext
        // Then:
        //      Use the nodes, parent.
        // Else:
        //      Use this node.
        public static DependencyObject GetDataContext(DependencyObject depObj, bool excludeNodesWithDcBinding, bool inspectAncestors = true)
        {
            bool hasDataContext = false;
            DependencyObject curObj = depObj;
            DependencyObject foundNode = depObj;

            do
            {
                foundNode = curObj;

                if (curObj is FrameworkElement fe)
                {
                    hasDataContext = HasDataContext(fe, out curObj, excludeNodesWithDcBinding: true);
                }
                else if(curObj is FrameworkContentElement fce)
                {
                    hasDataContext = HasDataContext(fce, out curObj, excludeNodesWithDcBinding: true);
                }
            }
            while (!hasDataContext && curObj != null && inspectAncestors);

            if(object.ReferenceEquals(foundNode, depObj))
            {
                System.Diagnostics.Debug.WriteLine("Logical Tree did not have to traverse any ancestors.");
            }

            return foundNode;
        }

        public static bool HasDataContext(DependencyObject depObj, out DependencyObject parent, bool excludeNodesWithDcBinding)
        {
            if (depObj is FrameworkElement fe)
            {
                parent = fe.Parent;
                return fe.DataContext != null && (!excludeNodesWithDcBinding || !HasDcBinding(depObj));
            }
            else if (depObj is FrameworkContentElement fce)
            {
                parent = fce.Parent;
                return  fce.DataContext != null && (!excludeNodesWithDcBinding || !HasDcBinding(depObj));
            }
            else
            {
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

        #endregion
    }
}
