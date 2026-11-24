using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace NCalc.Benchmarks
{
    internal static class AssemblyResolver
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            AssemblyLoadContext.Default.Resolving += static (context, assemblyName) =>
            {
                if (!string.Equals(assemblyName.Name, "Parlot", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var subFolder = Environment.Version.Major >= 10 ? "parlot-net10" : "parlot-net8";
                var dllPath = Path.Combine(AppContext.BaseDirectory, subFolder, "Parlot.dll");

                return File.Exists(dllPath) ? context.LoadFromAssemblyPath(dllPath) : null;
            };
        }
    }
}
