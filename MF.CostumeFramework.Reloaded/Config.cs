using MF.CostumeFramework.Reloaded.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;
using System.ComponentModel;

namespace MF.CostumeFramework.Reloaded.Configuration;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [DefaultValue(LogLevel.Information)]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [Category("Preferences")]
    [DisplayName("Use Costumes in Fields")]
    public bool UseFieldCostumes { get; set; }

    [Category("Preferences")]
    [DisplayName("Use Costumes in Events")]
    public bool UseEventCostumes { get; set; }
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}
