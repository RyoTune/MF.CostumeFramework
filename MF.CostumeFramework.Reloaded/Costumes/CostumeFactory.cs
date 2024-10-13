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
    private readonly ICriFsRedirectorApi _criFsApi = criFsApi;
    private readonly GameCostumes costumes = costumes;

    public Costume? Create(CostumeContext ctx, Character character, string costumeDir)
    {
        var config = GetCostumeConfig(costumeDir);
        var costume = CreateOrFindCostume(ctx.ModId, character, config.Name ?? Path.GetFileName(costumeDir));
        if (costume == null)
        {
            return null;
        }

        ApplyCostumeConfig(costume, config);
        LoadCostumeFiles(ctx, costume, costumeDir);
        //LoadCostumeRyo(costume, costumeDir);
        Log.Information($"Loaded Costume ({costume.Character}): {costume.Name} || ID: {costume.CostumeId} || Mod: {ctx.ModId}");
        return costume;
    }

    private static void ApplyCostumeConfig(Costume costume, CostumeConfig config)
    {
        IfNotNull(config.Name, value => costume.Config.Name = value);
    }

    private void LoadCostumeFiles(CostumeContext ctx, Costume costume, string costumeDir)
    {
        SetCostumeFile(Path.Join(costumeDir, "char_battle.gfs"), path => costume.Config.Battle.GfsPath = CreateBindPath(ctx, costumeDir, path));
        SetCostumeFile(Path.Join(costumeDir, "char_field.gfs"), path => costume.Config.Field.GfsPath = CreateBindPath(ctx, costumeDir, path));
        SetCostumeFile(Path.Join(costumeDir, "char_event.gfs"), path => costume.Config.Event.GfsPath = CreateBindPath(ctx, costumeDir, path));

        SetCostumeFile(Path.Join(costumeDir, "char_battle.tex"), path => costume.Config.Battle.TexPath = CreateBindPath(ctx, costumeDir, path));
        SetCostumeFile(Path.Join(costumeDir, "char_field.tex"), path => costume.Config.Field.TexPath = CreateBindPath(ctx, costumeDir, path));
        SetCostumeFile(Path.Join(costumeDir, "char_event.tex"), path => costume.Config.Event.TexPath = CreateBindPath(ctx, costumeDir, path));

        SetCostumeFile(Path.Join(costumeDir, "music.pme"), path => costume.MusicScriptFile = path);
        SetCostumeFile(Path.Join(costumeDir, "battle.theme.pme"), path => costume.BattleThemeFile = path);

        var itemMessages = msg.CreateItemMessages();

        costume.ItemMessageLabel = itemMessages.Label;
        itemMessages.EN.SetName(costume.Name);
        itemMessages.EN.SetDescription(costume.Description);
        foreach (var lang in Enum.GetValues<Language>())
        {
            if (lang == Language.Any) continue;
            SetCostumeFile(Path.Join(costumeDir, $"name.{lang.ToCode()}.msg"), path => itemMessages[lang].SetName(File.ReadAllText(path)));
            SetCostumeFile(Path.Join(costumeDir, $"description.{lang.ToCode()}.msg"), path => itemMessages[lang].SetDescription(File.ReadAllText(path)));
        }
    }

    private string CreateBindPath(CostumeContext ctx, string costumeDir, string assetFile)
    {
        var bindPath = Path.GetRelativePath(ctx.BaseDir, assetFile).Replace('\\', '/');
        _criFsApi.AddBind(bindPath, assetFile);
        return bindPath;
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
        var existingCostume = costumes.FirstOrDefault(x => x.Character == character && x.Name == name);
        if (existingCostume != null)
        {
            return existingCostume;
        }

        var newCostume = costumes.GetNewCostume();
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
