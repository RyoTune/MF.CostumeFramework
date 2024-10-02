using MF.CostumeFramework.Reloaded.Costumes.Models;

namespace MF.CostumeFramework.Reloaded.Utils;

internal class AssetUtils
{
    public static string GetAsssetPath(Character character, int costumeId, AssetType type)
        => type switch
        {
            AssetType.CharBattleGfs => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_B.GFS",
            AssetType.CharFieldGfs => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_F.GFS",
            AssetType.CharEventGfs => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_E.GFS",
            AssetType.CharBattleTex => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_B.TEX",
            AssetType.CharFieldTex => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_F.TEX",
            AssetType.CharEventTex => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_E.TEX",
            _ => throw new Exception(),
        };

    public static string GetCharIdString(Character character)
        => ((int)character).ToString("0000");

    public static string GetCostumeIdString(int costumeId)
        => costumeId.ToString("000");
}

internal enum AssetType
{
    // Character GFS
    CharBattleGfs,
    CharFieldGfs,
    CharEventGfs,

    // Character TEX
    CharBattleTex,
    CharFieldTex,
    CharEventTex,
}
