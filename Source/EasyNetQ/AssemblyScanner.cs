using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EasyNetQ
{
    internal static class AssemblyScanner
    {
        public static IEnumerable<Assembly> GetScannableAssemblies()
        {
            IEnumerable<FileInfo> iteratorVariable0 =
                new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFiles("*.dll", SearchOption.AllDirectories)
                    .Union(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFiles("*.exe",
                                                                                                       SearchOption.
                                                                                                           AllDirectories));
            foreach (FileInfo iteratorVariable1 in iteratorVariable0)
            {
                Assembly iteratorVariable2;
                try
                {
                    iteratorVariable2 = Assembly.LoadFrom(iteratorVariable1.FullName);
                    iteratorVariable2.GetTypes();
                }
                catch
                {
                    continue;
                }
                yield return iteratorVariable2;
            }

        }
    }
}