using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.TypeSafePropertyBag.LocalBinding
{
    public class BindingPathParser
    {
        public string[] GetPathElements(LocalBindingInfo bInfo, out bool pathIsAbsolute, out int firstNamedStepIndex)
        {
            string[] pathElements = bInfo.PropertyPath.Path.Split('/');
            int compCount = pathElements.Length;

            if (compCount == 0)
            {
                throw new InvalidOperationException("The path has no components.");
            }

            if (pathElements[compCount - 1] == "..")
            {
                throw new InvalidOperationException("The last component of the path cannot be '..'");
            }

            if (compCount == 1 && pathElements[0] == ".")
            {
                // Can't bind to yourself.
            }

            // Remove initial "this" component, if present.
            if (pathElements[0] == ".")
            {
                pathElements = RemoveFirstItem(pathElements);
                compCount--;
                if (pathElements[0] == ".") throw new InvalidOperationException("A path that starts with '././' is not supported.");
            }

            if (pathElements[0] == string.Empty)
            {
                pathIsAbsolute = true;

                // remove the initial (empty) path component.
                pathElements = RemoveFirstItem(pathElements);
                compCount--;

                if (pathElements[0] == "..") throw new InvalidOperationException("Absolute Paths cannot refer to a parent. (Path begins with '/../'.");
                firstNamedStepIndex = 0;

                // TODO: Listen to changes in the value of our node's root.
            }
            else
            {
                pathIsAbsolute = false;
                firstNamedStepIndex = GetFirstPathElementWithName(0, pathElements);

            }

            CheckForBadParRefs(firstNamedStepIndex + 1, pathElements);
            return pathElements;
        }

        private string[] RemoveFirstItem(string[] x)
        {
            string[] newElements = new string[x.Length - 1];
            Array.Copy(x, 1, newElements, 0, x.Length - 1);
            return newElements;
        }

        private int GetFirstPathElementWithName(int nPtr, string[] pathElements)
        {
            for (; nPtr < pathElements.Length; nPtr++)
            {
                if (pathElements[nPtr] != "..") break;
            }
            return nPtr;
        }

        private void CheckForBadParRefs(int nPtr, string[] pathElements)
        {
            for (; nPtr < pathElements.Length - 1; nPtr++)
            {
                if (pathElements[nPtr] == "..")
                {
                    throw new InvalidOperationException("A path cannot refer to a parent once a path element references a property by name.");
                }
            }
        }
    }
}
