using System;
using System.Collections.Generic;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    public class SimpleTypeResolver
    {
        Dictionary<string, Type> _typeCache;

        public SimpleTypeResolver()
        {
            _typeCache = new Dictionary<string, Type>();
        }

        public Type GetTypeFromName(string typeName)
        {
            if (!TryFindType(typeName, out Type result))
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.");
            }
            return result;
        }

        public bool TryFindType(string typeName, out Type t)
        {
            lock (_typeCache)
            {
                if (!_typeCache.TryGetValue(typeName, out t))
                {
                    t = GetTypeFromNameDirectly(typeName);

                    if (t == null)
                    {
                        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            t = GetTypeFromNameDirectly(a, typeName);
                            if (t != null) break;
                        }
                    }

                    _typeCache[typeName] = t; // perhaps null
                }
            }
            return t != null;
        }

        private Type GetTypeFromNameDirectly(string typeName)
        {
            try
            {
                Type result = Type.GetType(typeName);
                if (result == null)
                {
                    throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}. (No exception was thrown, but GetType returned null.");
                }

                return result;
            }
            catch (ArgumentNullException ane)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", ane);
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", tie);
            }
            catch (ArgumentException ae)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", ae);
            }
            catch (TypeLoadException tle)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", tle);
            }
            catch (System.IO.FileLoadException fle)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", fle);
            }
            catch (BadImageFormatException bife)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", bife);
            }
        }

        private Type GetTypeFromNameDirectly(Assembly a, string typeName)
        {
            try
            {
                Type result = Type.GetType(typeName);
                if (result == null)
                {
                    throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}. (No exception was thrown, but GetType returned null.");
                }

                return result;
            }
            catch (ArgumentNullException ane)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", ane);
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", tie);
            }
            catch (ArgumentException ae)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", ae);
            }
            catch (TypeLoadException tle)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", tle);
            }
            catch (System.IO.FileLoadException fle)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", fle);
            }
            catch (BadImageFormatException bife)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", bife);
            }
        }

    }
}
