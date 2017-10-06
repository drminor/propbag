using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DRM.PropBag.ControlsWPF.WPFHelpers
{
    public class LogicalTree
    {
        public static object GetDataContext(DependencyObject depObj, out DependencyObject foundNode)
        {
            object result = null;
            DependencyObject parent = depObj;
            foundNode = depObj;

            do
            {
                foundNode = parent;

                if (parent is FrameworkElement fe)
                {
                    result = GetDataContextInt(fe, out parent);
                }
                else if(parent is FrameworkContentElement fce)
                {
                    result = GetDataContextInt(fce, out parent);
                }
            }
            while (result == null && parent != null);

            if(object.ReferenceEquals(foundNode, depObj))
            {
                System.Diagnostics.Debug.WriteLine("Logical Tree did not have to traverse any ancestors.");
            }

            return result;
        }

        private static object GetDataContextInt(FrameworkElement fe, out DependencyObject parent)
        {
            parent = fe.Parent;
            return fe.DataContext;
        }

        private static object GetDataContextInt(FrameworkContentElement fce, out DependencyObject parent)
        {
            parent = fce.Parent;
            return fce.DataContext;
        }


    }
}
