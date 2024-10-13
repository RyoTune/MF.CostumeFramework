using MF.CostumeFramework.Reloaded.Costumes.Models;

namespace MF.CostumeFramework.Reloaded.Utils;

internal class AssetUtils
{
    public static string GetAsssetPath(Character character, int costumeId, CostumeAsset type)
        => type switch
        {
            CostumeAsset.CharBattleGfs => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_B.GFS",
            CostumeAsset.CharFieldGfs => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_F.GFS",
            CostumeAsset.CharEventGfs => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_E.GFS",
            CostumeAsset.CharBattleTex => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_B.TEX",
            CostumeAsset.CharFieldTex => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_F.TEX",
            CostumeAsset.CharEventTex => $"COMMON/model/character/{GetCharIdString(character)}/c_{GetCharIdString(character)}_{GetCostumeIdString(costumeId)}_E.TEX",
            _ => throw new Exception(),
        };

    public static string GetCharIdString(Character character)
        => ((int)character).ToString("0000");

    public static string GetCostumeIdString(int costumeId)
        => costumeId.ToString("000");
}

internal enum CostumeAsset
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
