using CriFs.V2.Hook.Interfaces;
using MF.CostumeFramework.Reloaded.Configuration;
using MF.CostumeFramework.Reloaded.Costumes;
using MF.CostumeFramework.Reloaded.Template;
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

    private readonly IModLoader modLoader;
    private readonly IReloadedHooks? hooks;
    private readonly ILogger log;
    private readonly IMod owner;

    private Config config;
    private readonly IModConfig modConfig;

    private readonly CostumeRegistry costumeRegistry;
    private readonly CostumeService costumeService;

    public Mod(ModContext context)
    {
        this.modLoader = context.ModLoader;
        this.hooks = context.Hooks;
        this.log = context.Logger;
        this.owner = context.Owner;
        this.config = context.Configuration;
        this.modConfig = context.ModConfig;
#if DEBUG
        Debugger.Launch();
#endif

        Project.Init(this.modConfig, this.modLoader, this.log, true);
        Log.LogLevel = this.config.LogLevel;

        this.modLoader.GetController<ICriFsRedirectorApi>().TryGetTarget(out var criFsApi);
        this.modLoader.GetController<IMetaphorLibrary>().TryGetTarget(out var metaphor);
        this.modLoader.GetController<IMessage>().TryGetTarget(out var msg);

        this.costumeRegistry = new CostumeRegistry(criFsApi!);
        this.costumeService = new CostumeService(metaphor!, msg!, this.costumeRegistry);

        this.modLoader.ModLoaded += this.OnModLoaded;
        Project.Start();
    }

    private void OnModLoaded(IModV1 mod, IModConfigV1 config)
    {
        var modDir = this.modLoader.GetDirectoryForModId(config.ModId);
        var costumesDir = Path.Join(modDir, "costumes");
        if (!Directory.Exists(costumesDir))
        {
            return;
        }

        this.costumeRegistry.AddCostumesFolder(config.ModId, costumesDir);
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        config = configuration;
        log.WriteLine($"[{modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}