using CriFs.V2.Hook.Interfaces;
using MF.CostumeFramework.Reloaded.Costumes.Models;
using MF.CostumeFramework.Reloaded.Utils;
using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Interfaces.Messages;
using static MF.CostumeFramework.Reloaded.Utils.ModUtils;

namespace MF.CostumeFramework.Reloaded.Costumes;

internal class CostumeFactory(ICriFsRedirectorApi criFsApi, IMessage msg, GameCostumes costumes)
{
    private readonly IMessage msg = msg;
    private readonly ICriFsRedirectorApi criFsApi = criFsApi;
    private readonly GameCostumes costumes = costumes;

    public Costume? Create(CostumeContext ctx, Character character, string costumeDir)
    {
        var config = GetCostumeConfig(costumeDir);
        var costume = this.CreateOrFindCostume(ctx.ModId, character, config.Name ?? Path.GetFileName(costumeDir));
        if (costume == null)
        {
            return null;
        }

        ApplyCostumeConfig(costume, config);
        LoadCostumeFiles(costume, costumeDir);
        //LoadCostumeRyo(costume, costumeDir);
        this.BindAssets(costume);
        Log.Information($"Loaded Costume ({costume.Character}): {costume.Name} || ID: {costume.CostumeId} || Mod: {ctx.ModId}");
        return costume;
    }

    private static void ApplyCostumeConfig(Costume costume, CostumeConfig config)
    {
        IfNotNull(config.Name, value => costume.Config.Name = value);
    }

    private void LoadCostumeFiles(Costume costume, string costumeDir)
    {
        SetCostumeFile(Path.Join(costumeDir, "char_battle.gfs"), path => costume.Config.Battle.GfsPath = path);
        SetCostumeFile(Path.Join(costumeDir, "char_field.gfs"), path => costume.Config.Field.GfsPath = path);
        SetCostumeFile(Path.Join(costumeDir, "char_event.gfs"), path => costume.Config.Event.GfsPath = path);

        SetCostumeFile(Path.Join(costumeDir, "char_battle.tex"), path => costume.Config.Battle.TexPath = path);
        SetCostumeFile(Path.Join(costumeDir, "char_field.tex"), path => costume.Config.Field.TexPath = path);
        SetCostumeFile(Path.Join(costumeDir, "char_event.tex"), path => costume.Config.Event.TexPath = path);

        SetCostumeFile(Path.Join(costumeDir, "music.pme"), path => costume.MusicScriptFile = path);
        SetCostumeFile(Path.Join(costumeDir, "battle.theme.pme"), path => costume.BattleThemeFile = path);

        var itemMessages = msg.CreateItemMessages();

        costume.ItemMessageLabel = itemMessages.Label;
        itemMessages.EN.SetName(costume.Name);
        itemMessages.EN.SetDescription(costume.Description);
        foreach (var lang in Enum.GetValues<Language>())
        {
            SetCostumeFile(Path.Join(costumeDir, $"name.{lang.ToCode()}.msg"), path => itemMessages[lang].SetName(File.ReadAllText(path)));
            SetCostumeFile(Path.Join(costumeDir, $"description.{lang.ToCode()}.msg"), path => itemMessages[lang].SetDescription(File.ReadAllText(path)));
        }
    }

    private void BindAssets(Costume costume)
    {
        IfNotNull(costume.Config.Battle.GfsPath, path => this.BindAsset(costume, path!, AssetType.CharBattleGfs));
        IfNotNull(costume.Config.Field.GfsPath, path => this.BindAsset(costume, path!, AssetType.CharFieldGfs));
        IfNotNull(costume.Config.Event.GfsPath, path => this.BindAsset(costume, path!, AssetType.CharEventGfs));

        IfNotNull(costume.Config.Battle.TexPath, path => this.BindAsset(costume, path!, AssetType.CharBattleTex));
        IfNotNull(costume.Config.Field.TexPath, path => this.BindAsset(costume, path!, AssetType.CharFieldTex));
        IfNotNull(costume.Config.Event.TexPath, path => this.BindAsset(costume, path!, AssetType.CharEventTex));
    }

    private void BindAsset(Costume costume, string assetPath, AssetType type)
    {
        var ogPath = AssetUtils.GetAsssetPath(costume.Character, costume.CostumeId, type);
        this.criFsApi.AddBind(ogPath, assetPath);
    }

    private static void SetCostumeFile(string modFile, Action<string> setFile)
    {
        if (File.Exists(modFile))
        {
            setFile(modFile);
        }
    }

    /// <summary>
    /// Creates a new costume for <paramref name="character"/> or gets an existing costume by <paramref name="name"/>.
    /// </summary>
    private Costume? CreateOrFindCostume(string ownerId, Character character, string name)
    {
        var existingCostume = this.costumes.FirstOrDefault(x => x.Character == character && x.Name == name);
        if (existingCostume != null)
        {
            return existingCostume;
        }

        var newCostume = this.costumes.GetNewCostume();
        if (newCostume != null)
        {
            newCostume.Name = name;
            newCostume.Character = character;
            newCostume.IsEnabled = true;
            newCostume.OwnerModId = ownerId;
        }
        else
        {
            Log.Warning("No new costume slots available.");
        }

        return newCostume;
    }

    private static CostumeConfig GetCostumeConfig(string costumeDir)
    {
        var configFile = Path.Join(costumeDir, "config.yaml");
        if (File.Exists(configFile))
        {
            return YamlSerializer.DeserializeFile<CostumeConfig>(configFile);
        }

        return new();
    }
}
