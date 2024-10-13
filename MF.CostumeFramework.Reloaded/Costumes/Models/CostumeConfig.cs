using MF.CostumeFramework.Reloaded.Utils;

namespace MF.CostumeFramework.Reloaded.Costumes.Models;

internal class CostumeConfig
{
    public string? Name { get; set; }

    public Model Battle { get; } = new();

    public Model Field { get; } = new();

    public Model Event { get; } = new();

    public string? GetAssetPath(CostumeAsset assetType)
     => assetType switch
     {
         CostumeAsset.CharBattleGfs => GetOrParseAssetPath(Battle.GfsPath),
         CostumeAsset.CharFieldGfs => GetOrParseAssetPath(Field.GfsPath),
         CostumeAsset.CharEventGfs => GetOrParseAssetPath(Event.GfsPath),
         CostumeAsset.CharBattleTex => GetOrParseAssetPath(Battle.TexPath),
         CostumeAsset.CharFieldTex => GetOrParseAssetPath(Field.TexPath),
         CostumeAsset.CharEventTex => GetOrParseAssetPath(Event.TexPath),
         _ => throw new NotImplementedException(),
     };

    private static string? GetOrParseAssetPath(string? assetPath)
    {
        if (assetPath == null)
        {
            return null;
        }

        if (assetPath.StartsWith("asset:"))
        {
            var parts = assetPath["asset:".Length..].Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                return null;
            }

            var character = Enum.Parse<Character>(parts[0], true);
            var type = Enum.Parse<CostumeAsset>(parts[1], true);
            var costumeId = 0;
            if (parts.Length == 3)
            {
                _ = int.TryParse(parts[2], out costumeId);
            }

            return AssetUtils.GetAsssetPath(character, costumeId, type);
        }

        return assetPath;
    }
}

internal class Model
{
    public string? GfsPath { get; set; }

    public string? TexPath { get; set; }
}
