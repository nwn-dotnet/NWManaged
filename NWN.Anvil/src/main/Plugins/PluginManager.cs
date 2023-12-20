using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Anvil.API;
using Anvil.Internal;
using Anvil.Services;
using NLog;

namespace Anvil.Plugins
{
  /// <summary>
  /// Loads all available plugins and their types for service initialisation.
  /// </summary>
  [ServiceBindingOptions(InternalBindingPriority.AboveNormal)]
  public sealed class PluginManager : ICoreService
  {
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private const int PluginUnloadAttempts = 10;
    private const int PluginUnloadSleepMs = 5000;

    [Inject]
    private InjectionService InjectionService { get; init; } = null!;

    internal List<Plugin> Plugins { get; } = new List<Plugin>();

    /// <summary>
    /// Gets the install directory of the specified plugin.
    /// </summary>
    /// <param name="pluginAssembly">The assembly of the plugin, e.g. typeof(MyService).Assembly</param>
    /// <returns>The install directory for the specified plugin.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified assembly is not a plugin.</exception>
    public string? GetPluginDirectory(Assembly pluginAssembly)
    {
      if (IsPluginAssembly(pluginAssembly))
      {
        return Path.GetDirectoryName(pluginAssembly.Location);
      }

      throw new ArgumentException("Specified assembly is not a loaded plugin assembly.", nameof(pluginAssembly));
    }

    /// <summary>
    /// Gets if the specified assembly is the primary assembly for a plugin.
    /// </summary>
    /// <param name="assembly">The assembly to query.</param>
    /// <returns>True if the assembly is a plugin, otherwise false.</returns>
    public bool IsPluginAssembly(Assembly assembly)
    {
      return Plugins.Any(plugin => plugin.Assembly == assembly);
    }

    [Obsolete("Use GetPlugin().IsLoaded instead.")]
    public bool IsPluginLoaded(string pluginName)
    {
      return GetPlugin(pluginName)?.IsLoaded == true;
    }

    /// <summary>
    /// Gets the plugin from the specified name.
    /// </summary>
    /// <returns>The associated plugin, otherwise null.</returns>
    public Plugin? GetPlugin(string pluginName)
    {
      return Plugins.FirstOrDefault(plugin => plugin.Name.Name == pluginName);
    }

    /// <summary>
    /// Locates the plugin associated with the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to search.</param>
    /// <returns>The associated plugin, otherwise null.</returns>
    public Plugin? GetPlugin(Assembly assembly)
    {
      return Plugins.FirstOrDefault(plugin => plugin.Assembly == assembly);
    }

    internal Assembly? ResolveDependency(string pluginName, AssemblyName dependencyName)
    {
      Assembly? assembly = ResolveDependencyFromAnvil(pluginName, dependencyName);
      if (assembly == null)
      {
        assembly = ResolveDependencyFromPlugins(pluginName, dependencyName);
      }

      return assembly;
    }

    void ICoreService.Init() {}

    void ICoreService.Load()
    {
      BootstrapPlugins();
      LoadPlugins(false);
    }

    void ICoreService.Shutdown() {}

    void ICoreService.Start()
    {
      LoadPlugins(true);
    }

    void ICoreService.Unload()
    {
      Log.Info("Unloading plugins...");
      Dictionary<WeakReference, string> pendingUnloads = new Dictionary<WeakReference, string>();
      foreach (Plugin plugin in Plugins)
      {
        Log.Info("Unloading DotNET plugin {PluginName} - {PluginPath}", plugin.Name.Name, plugin.Path);
        pendingUnloads.Add(plugin.Unload(), plugin.Name.Name!);
      }

      Plugins.Clear();
      Plugins.TrimExcess();

      if (EnvironmentConfig.ReloadEnabled)
      {
        for (int unloadAttempt = 1; !IsUnloadComplete(pendingUnloads, unloadAttempt); unloadAttempt++)
        {
          GC.Collect();
          GC.WaitForPendingFinalizers();

          if (unloadAttempt > PluginUnloadAttempts)
          {
            Thread.Sleep(PluginUnloadSleepMs);
          }
        }
      }
    }

    private void BootstrapPlugins()
    {
      List<IPluginSource> pluginSources = new List<IPluginSource>
      {
        InjectionService.Inject(new PaketPluginSource()),
        InjectionService.Inject(new LocalPluginSource(HomeStorage.Plugins)),
      };

      foreach (string pluginPath in EnvironmentConfig.AdditionalPluginPaths)
      {
        string fullPluginPath = Path.GetFullPath(pluginPath, NwServer.Instance.UserDirectory);
        if (Directory.Exists(fullPluginPath))
        {
          pluginSources.Add(InjectionService.Inject(new LocalPluginSource(fullPluginPath)));
        }
      }

      foreach (IPluginSource pluginSource in pluginSources)
      {
        Plugins.AddRange(pluginSource.Bootstrap());
      }

      if (EnvironmentConfig.PreventStartNoPlugin && Plugins.Count == 0)
      {
        throw new Exception("No plugins are available to load, and ANVIL_PREVENT_START_NO_PLUGIN is enabled.\n" +
          $"Check your plugins are available at {HomeStorage.Plugins}, or add valid plugins paths using the ANVIL_ADD_PLUGIN_PATHS variable.");
      }
    }

    private bool IsUnloadComplete(Dictionary<WeakReference, string> pendingUnloads, int attempt)
    {
      bool retVal = true;
      foreach ((WeakReference? assemblyReference, string? pluginName) in pendingUnloads)
      {
        if (assemblyReference.IsAlive)
        {
          if (attempt > PluginUnloadAttempts)
          {
            Log.Warn("Plugin {PluginName} is preventing unload", pluginName);
          }

          retVal = false;
        }
      }

      return retVal;
    }

    private bool IsValidDependency(string plugin, AssemblyName requested, AssemblyName resolved)
    {
      if (requested.Name != resolved.Name)
      {
        return false;
      }

      if (requested.Version != resolved.Version)
      {
        Log.Warn("DotNET Plugin {Plugin} references {ReferenceName}, v{ReferenceVersion} but the server is running v{ServerVersion}! You may encounter compatibility issues",
          plugin,
          requested.Name,
          requested.Version,
          resolved.Version);
      }

      return true;
    }

    private void LoadPlugin(Plugin plugin)
    {
      string isolated = plugin.PluginInfo.Isolated ? " isolated " : " ";

      Log.Info($"Loading{isolated}DotNET plugin {plugin.Name.Name} {plugin.Name.Version} - {plugin.Path}");

      plugin.Load();

      if (plugin.Assembly != null)
      {
        Log.Info($"Loaded{isolated}DotNET plugin {plugin.Name.Name} {plugin.Name.Version} - {plugin.Path}");
      }
      else
      {
        Log.Error($"Failed to load{isolated}DotNET plugin {plugin.Name.Name} {plugin.Name.Version} - {plugin.Path}");
      }
    }

    private void LoadPlugins(bool isolated)
    {
      foreach (Plugin plugin in Plugins)
      {
        if (plugin.IsLoaded || plugin.PluginInfo.Isolated != isolated)
        {
          continue;
        }

        if (EnvironmentConfig.GetIsPluginDisabled(plugin.Name.Name!))
        {
          Log.Info($"Skipping loading DotNET plugin due to configuration: {plugin.Name.Name} {plugin.Name.Version} - {plugin.Path}");
          continue;
        }

        LoadPlugin(plugin);
      }
    }

    private Assembly? ResolveDependencyFromAnvil(string pluginName, AssemblyName dependencyName)
    {
      foreach (Assembly assembly in Assemblies.AllAssemblies)
      {
        if (IsValidDependency(pluginName, dependencyName, assembly.GetName()))
        {
          return assembly;
        }
      }

      return null;
    }

    private Assembly? ResolveDependencyFromPlugins(string pluginName, AssemblyName dependencyName)
    {
      foreach (Plugin plugin in Plugins)
      {
        if (!IsValidDependency(pluginName, dependencyName, plugin.Name))
        {
          continue;
        }

        if (plugin.Loading)
        {
          Log.Error("DotNET plugins {PluginA} <--> {PluginB} cannot be loaded as they have circular dependencies", pluginName, plugin.Name.Name);
          return null;
        }

        if (!plugin.IsLoaded)
        {
          LoadPlugin(plugin);
        }

        return plugin.Assembly;
      }

      return null;
    }
  }
}
