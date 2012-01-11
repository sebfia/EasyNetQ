using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyNetQ
{
    public class ScanAssemblies
    {
        private readonly List<Assembly> _scannedAssemblies; 
        private static readonly ScanAssemblies _instance;

        protected ScanAssemblies()
        {
            _scannedAssemblies = AssemblyScanner.GetScannableAssemblies().ToList();
        }

        static ScanAssemblies()
        {
            _instance = new ScanAssemblies();
        }

        public static IEnumerable<Type> For<T>()
        {
            return For(typeof (T));
        }

        public static IEnumerable<Type> For(Type typeToScanFor)
        {
            return _instance._scannedAssemblies.SelectMany(
                assembly =>
                (from t in assembly.GetTypes()
                 where typeToScanFor.IsAssignableFrom(t) && (t != typeToScanFor) ||
                 ImplementsGenericInterface(t, typeToScanFor)
                 select t));
        }

        private static bool ImplementsGenericInterface(Type type, Type interfaceType)
        {
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }
    }
}