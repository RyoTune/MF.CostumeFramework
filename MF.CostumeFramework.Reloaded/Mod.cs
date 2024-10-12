using CriFs.V2.Hook.Interfaces;
using MF.CostumeFramework.Reloaded.Common;
using MF.CostumeFramework.Reloaded.Configuration;
using MF.CostumeFramework.Reloaded.Costumes;
using MF.CostumeFramework.Reloaded.Template;
using MF.Toolkit.Interfaces.Inventory;
using MF.Toolkit.Interfaces.Library;
using MF.Toolkit.Interfaces.Messages;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Diagnostics;

namespace MF.CostumeFramework.Reloaded;

public class Mod : ModBase
{
    public const string NAME = "MF.CostumeFramework.Reloaded";

    private readonly IModLoader _modLoader;
    private readonly IReloadedHooks? hooks;
    private readonly ILogger log;
    private readonly IMod owner;

    private Config config;
    private readonly IModConfig modConfig;

    private readonly CostumeRegistry _costumeRegistry;
    private readonly CostumeService _costumeService;
    private readonly List<IUseConfig> _configurables = [];

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        hooks = context.Hooks;
        log = context.Logger;
        owner = context.Owner;
        config = context.Configuration;
        modConfig = context.ModConfig;
#if DEBUG
        Debugger.Launch();
#endif

        Project.Init(modConfig, _modLoader, log, true);
        Log.LogLevel = config.LogLevel;

        _modLoader.GetController<ICriFsRedirectorApi>().TryGetTarget(out var criFsApi);
        _modLoader.GetController<IMetaphorLibrary>().TryGetTarget(out var metaphor);
        _modLoader.GetController<IMessage>().TryGetTarget(out var msg);
        _modLoader.GetController<IInventory>().TryGetTarget(out var inv);

        _costumeRegistry = new CostumeRegistry(criFsApi!, msg!);
        _costumeService = new CostumeService(metaphor!, msg!, inv!, _costumeRegistry);
        _configurables.Add(_costumeService);

        _modLoader.ModLoaded += OnModLoaded;
        ConfigurationUpdated(config);
        Project.Start();
    }

    private void OnModLoaded(IModV1 mod, IModConfigV1 config)
    {
        var modDir = _modLoader.GetDirectoryForModId(config.ModId);
        var costumesDir = Path.Join(modDir, "costumes");
        if (!Directory.Exists(costumesDir))
        {
            return;
        }

        _costumeRegistry.AddCostumesFolder(config.ModId, costumesDir);
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        config = configuration;
        log.WriteLine($"[{modConfig.ModId}] Config Updated: Applying");
        Log.LogLevel = config.LogLevel;
        foreach (var configurable in _configurables)
        {
            configurable.UpdateConfig(config);
        }
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}