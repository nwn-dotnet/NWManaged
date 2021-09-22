using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Anvil.Internal
{
  internal static class Assemblies
  {
    internal static readonly Assembly Anvil = typeof(Assemblies).Assembly;
    internal static readonly Assembly Core = typeof(NWN.Core.NWNCore).Assembly;
    internal static readonly Assembly Native = typeof(NWN.Native.API.NWNXLib).Assembly;

    internal static readonly string AssemblyDir = Path.GetDirectoryName(Anvil.Location);

    public static readonly Assembly[] AllAssemblies =
    {
      Anvil,
      Core,
      Native,
      typeof(NLog.Logger).Assembly,
      typeof(LightInject.ServiceContainer).Assembly,
      typeof(Newtonsoft.Json.JsonConvert).Assembly,
    };

    public static readonly List<string> ReservedNames = AllAssemblies
      .Select(assembly => assembly.GetName().Name)
      .ToList();
  }
}
