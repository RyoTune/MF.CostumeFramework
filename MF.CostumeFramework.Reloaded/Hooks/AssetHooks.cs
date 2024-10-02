using MF.CostumeFramework.Reloaded.Costumes;
using MF.CostumeFramework.Reloaded.Costumes.Models;
using MF.Toolkit.Interfaces.Library;
using Reloaded.Hooks.Definitions;
using System.Runtime.InteropServices;

namespace MF.CostumeFramework.Reloaded.Hooks;

internal class AssetHooks
{
    private delegate nint GetAsset(nint param1, nint param2, AssetType assetType, int majorId, int minorId, int subId);
    private IHook<GetAsset>? getAssetHook;
    private IHook<GetAsset>? getTexHook;

    private readonly IMetaphorLibrary mf;
    private readonly CostumeRegistry registry;

    public AssetHooks(IMetaphorLibrary mf, CostumeRegistry registry)
    {
        this.mf = mf;
        this.registry = registry;

        ScanHooks.Add(
            nameof(GetAsset),
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 70 41 8B D9",
            (hooks, result) => this.getAssetHook = hooks.CreateHook<GetAsset>((a, b, c, d, e, f) => this.GetAssetImpl(a, b, c, d, e, f, false), result).Activate());

        ScanHooks.Add(
            $"{nameof(GetAsset)}: TEX",
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 41 8B D9 41 8B F0",
            (hooks, result) => this.getTexHook = hooks.CreateHook<GetAsset>((a, b, c, d, e, f) => this.GetAssetImpl(a, b, c, d, e, f, true), result).Activate());
    }

    private nint GetAssetImpl(nint param1, nint param2, AssetType assetType, int majorId, int minorId, int subId, bool isTex)
    {
        if (assetType == AssetType.CharacterGfs_B && majorId == 1)
        {
            var costumeItemId = this.mf.CharaGetEquip(1, 4) - CostumeItem.BASE_ITEM_ID;
            if (this.registry.TryGetCostumeByItemId(Character.Player, costumeItemId, out var costume))
            {
                minorId = costume.CostumeId;
                Log.Information($"Using Costume || {Character.Player} || Costume: {costume.Name}");
            }
        }

        var result = isTex ? this.getTexHook!.OriginalFunction(param1, param2, assetType, majorId, minorId, subId)
            : this.getAssetHook!.OriginalFunction(param1, param2, assetType, majorId, minorId, subId);

        return result;
    }

    private enum AssetType
    {
        Camera = 20,
        Icon = 11,
        Layout = 12,
        Env = 15,
        FCMR = 25,
        Object13 = 13,
        Object14 = 14,
        FieldEffect = 10,
        CharacterGfs_B = 1,
        CharacterGfs_F = 2,
        ModelWeapon = 16,
        ModelArchetype = 33,
        ModelEnemy = 6,
    }
}
