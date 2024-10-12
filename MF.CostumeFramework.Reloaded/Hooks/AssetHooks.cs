using MF.CostumeFramework.Reloaded.Common;
using MF.CostumeFramework.Reloaded.Configuration;
using MF.CostumeFramework.Reloaded.Costumes;
using MF.CostumeFramework.Reloaded.Costumes.Models;
using MF.Toolkit.Interfaces.Library;
using Reloaded.Hooks.Definitions;

namespace MF.CostumeFramework.Reloaded.Hooks;

internal class AssetHooks : IUseConfig
{
    private delegate nint GetAsset(nint param1, nint param2, AssetType assetType, int majorId, int minorId, int subId, int param7);
    private IHook<GetAsset>? getAssetHook;
    private IHook<GetAsset>? getTexHook;

    private readonly IMetaphorLibrary mf;
    private readonly CostumeRegistry registry;

    private bool _useFieldCostumes;
    private bool _useEventCostumes;

    public AssetHooks(IMetaphorLibrary mf, CostumeRegistry registry)
    {
        this.mf = mf;
        this.registry = registry;

        ScanHooks.Add(
            nameof(GetAsset),
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 70 41 8B D9",
            (hooks, result) => this.getAssetHook = hooks.CreateHook<GetAsset>((a, b, c, d, e, f, g) => this.GetAssetImpl(a, b, c, d, e, f, g, false), result).Activate());

        ScanHooks.Add(
            $"{nameof(GetAsset)}: TEX",
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 41 8B D9 41 8B F0",
            (hooks, result) => this.getTexHook = hooks.CreateHook<GetAsset>((a, b, c, d, e, f, g) => this.GetAssetImpl(a, b, c, d, e, f, g, true), result).Activate());
    }

    private nint GetAssetImpl(nint param1, nint param2, AssetType assetType, int majorId, int minorId, int subId, int param7, bool isTex)
    {
        Log.Verbose($"{nameof(GetAsset)} || {assetType} || {majorId} || {minorId} || {subId} || {param7}");

        var ogResult = isTex ? this.getTexHook!.OriginalFunction(param1, param2, assetType, majorId, minorId, subId, param7)
            : this.getAssetHook!.OriginalFunction(param1, param2, assetType, majorId, minorId, subId, param7);

        if (!IsCharModel(assetType))
        {
            return ogResult;
        }

        var character = (Character)majorId;
        if (!Enum.IsDefined(character))
        {
            return ogResult;
        }

        Log.Debug($"{nameof(GetAsset)} || {assetType} || {character} || {minorId} || {subId}");

        var costumeItemId = this.mf.CharaGetEquip((int)character, 4) - CostumeItem.BASE_ITEM_ID;
        if (this.registry.TryGetCostumeByItemId(character, costumeItemId, out var costume))
        {
            switch (assetType)
            {
                case AssetType.CharacterGfs_B:
                    if (costume.Config.Battle.GfsPath != null)
                    {
                        minorId = costume.CostumeId;
                        Log.Debug($"Costume (Battle): {costume.Character} || {costume.Name} || {costume.CostumeId}");
                    }
                    break;

                // Use GFS provided by costume for fields and events
                // or use battle GFS if forcing costumes.
                case AssetType.CharacterGfs_F_2:
                case AssetType.CharacterGfs_F_5:
                    if (costume.Config.Field.GfsPath != null)
                    {
                        minorId = costume.CostumeId;
                        Log.Debug($"Costume (Field): {costume.Character} || {costume.Name} || {costume.CostumeId}");
                    }
                    else if (_useFieldCostumes && costume.Config.Battle.GfsPath != null)
                    {
                        minorId = costume.CostumeId;
                        assetType = AssetType.CharacterGfs_B;
                        Log.Debug($"Costume (Field->Battle): {costume.Character} || {costume.Name} || {costume.CostumeId}");
                    }
                    break;
                case AssetType.CharacterGfs_E:
                    if (costume.Config.Event.GfsPath != null)
                    {
                        minorId = costume.CostumeId;
                        Log.Debug($"Costume (Event): {costume.Character} || {costume.Name} || {costume.CostumeId}");
                    }
                    else if (_useEventCostumes && costume.Config.Battle.GfsPath != null)
                    {
                        minorId = costume.CostumeId;
                        assetType = AssetType.CharacterGfs_B;
                        Log.Debug($"Costume (Event->Battle): {costume.Character} || {costume.Name} || {costume.CostumeId}");
                    }
                    break;
            }

            Log.Information($"Redirected Asset || {character} || Costume: {costume.Name} || {costume.CostumeId}");
        }

        return isTex ? this.getTexHook!.OriginalFunction(param1, param2, assetType, majorId, minorId, subId, param7)
            : this.getAssetHook!.OriginalFunction(param1, param2, assetType, majorId, minorId, subId, param7);
    }

    public void UpdateConfig(Config config)
    {
        _useFieldCostumes = config.UseFieldCostumes;
        _useEventCostumes = config.UseEventCostumes;
    }

    private static bool IsCharModel(AssetType type)
        => type == AssetType.CharacterGfs_E
        || type == AssetType.CharacterGfs_F_2
        || type == AssetType.CharacterGfs_F_5
        || type == AssetType.CharacterGfs_B;

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
        CharacterGfs_F_2 = 2,
        CharacterGfs_F_5 = 5,
        CharacterGfs_E = 35,
        ModelWeapon = 16,
        ModelArchetype = 33,
        ModelEnemy = 6,
    }
}
