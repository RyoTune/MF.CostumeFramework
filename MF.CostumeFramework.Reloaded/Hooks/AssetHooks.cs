using MF.CostumeFramework.Reloaded.Common;
using MF.CostumeFramework.Reloaded.Configuration;
using MF.CostumeFramework.Reloaded.Costumes;
using MF.CostumeFramework.Reloaded.Costumes.Models;
using MF.CostumeFramework.Reloaded.Utils;
using MF.Toolkit.Interfaces.Library;
using Reloaded.Hooks.Definitions;
using System.Runtime.InteropServices;
using System.Text;

namespace MF.CostumeFramework.Reloaded.Hooks;

internal unsafe class AssetHooks : IUseConfig
{
    private delegate void GetAsset(nint param1, nint param2, AssetType assetType, int majorId, int minorId, int subId, int param7);
    private IHook<GetAsset>? _getAssetHook;
    private IHook<GetAsset>? _getTexHook;

    private delegate void UpdateAsset(nint param1);
    private IHook<UpdateAsset>? _updateAssetHook;

    private readonly IMetaphorLibrary _mf;
    private readonly CostumeRegistry _registry;

    private bool _useFieldCostumes;
    private bool _useEventCostumes;

    public AssetHooks(IMetaphorLibrary mf, CostumeRegistry registry)
    {
        _mf = mf;
        _registry = registry;

        ScanHooks.Add(
            nameof(GetAssetPath),
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 70 41 8B D9",
            (hooks, result) => _getAssetHook = hooks.CreateHook<GetAsset>((a, b, c, d, e, f, g) => GetAssetImpl(a, b, c, d, e, f, g, false), result).Activate());

        ScanHooks.Add(
            $"{nameof(GetAssetPath)}: TEX",
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 41 8B D9 41 8B F0",
            (hooks, result) => _getTexHook = hooks.CreateHook<GetAsset>((a, b, c, d, e, f, g) => GetAssetImpl(a, b, c, d, e, f, g, true), result).Activate());

        ScanHooks.Add(nameof(UpdateAsset), "40 55 53 56 57 41 55 41 56 48 8D 6C 24 ?? 48 81 EC 28 01 00 00", (hooks, result) => _updateAssetHook = hooks.CreateHook<UpdateAsset>(UpdateAssetImpl, result));
    }

    private void UpdateAssetImpl(nint param1)
    {
        var uVar15 = *(nint*)param1;
        var assetType = (AssetType)(uVar15 >> 0x3a);
        var majorId = (int)(uVar15 >> 0x14 & 0xffff);
        var minorId = (int)uVar15 & 0x3ff;

        /*
         * param2 = (uVar15 >> 0x37) & 7
         * assetType = uVar15 >> 0x3a
         * majorId = uVar15 >> 0x14 & 0xffff
         * minorId = (uint)uVar15 & 0x3ff
         * subId = (uint)(uVar15 >> 10) & 0x3ff
         */

        //Log.Information($"{nameof(UpdateAsset)} || {(AssetType)assetType} || {majorId} || {minorId}");
        if (majorId == 2 && assetType == AssetType.CharacterGfs_B)
        {
            uVar15 &= ~0x3ff;
            uVar15 |= (nint)1 & 0x3ff;
            *(nint*)param1 = uVar15;
            Log.Information($"{param1:X}");
        }

        _updateAssetHook!.OriginalFunction(param1);
    }

    private void GetAssetImpl(nint buffer, nint param2, AssetType assetType, int majorId, int minorId, int subId, int param7, bool isTex)
    {
        Log.Verbose($"{nameof(GetAssetPath)} || {assetType} || {majorId} || {minorId} || {subId} || {param7}");

        if (isTex)
            _getTexHook!.OriginalFunction(buffer, param2, assetType, majorId, minorId, subId, param7);
        else
            _getAssetHook!.OriginalFunction(buffer, param2, assetType, majorId, minorId, subId, param7);

        if (!IsCharModel(assetType))
        {
            return;
        }

        var character = (Character)majorId;
        if (!Enum.IsDefined(character))
        {
            return;
        }

        Log.Debug($"{nameof(GetAssetPath)} || {assetType} || {character} || {minorId} || {subId}");

        var costumeItemId = _mf.CharaGetEquip((int)character, 4) - CostumeItem.BASE_ITEM_ID;
        string? newAssetPath = null;
        if (_registry.TryGetCostumeByItemId(character, costumeItemId, out var costume))
        {
            switch (assetType)
            {
                case AssetType.CharacterGfs_B:
                    if (costume.Config.Battle.GfsPath != null)
                    {
                        minorId = costume.CostumeId;
                        Log.Debug($"Costume (Battle): {costume.Character} || {costume.Name} || {costume.CostumeId}");
                        newAssetPath = GetAssetPath(costume, assetType, isTex);
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
                        newAssetPath = GetAssetPath(costume, assetType, isTex);
                    }
                    else if (_useFieldCostumes && costume.Config.Battle.GfsPath != null)
                    {
                        minorId = costume.CostumeId;
                        //assetType = AssetType.CharacterGfs_B;
                        Log.Debug($"Costume (Field->Battle): {costume.Character} || {costume.Name} || {costume.CostumeId}");
                        newAssetPath = GetAssetPath(costume, AssetType.CharacterGfs_B, isTex);
                    }
                    break;
                case AssetType.CharacterGfs_E:
                    if (costume.Config.Event.GfsPath != null)
                    {
                        minorId = costume.CostumeId;
                        Log.Debug($"Costume (Event): {costume.Character} || {costume.Name} || {costume.CostumeId}");
                        newAssetPath = GetAssetPath(costume, assetType, isTex);
                    }
                    else if (_useEventCostumes && costume.Config.Battle.GfsPath != null)
                    {
                        minorId = costume.CostumeId;
                        //assetType = AssetType.CharacterGfs_B;
                        Log.Debug($"Costume (Event->Battle): {costume.Character} || {costume.Name} || {costume.CostumeId}");
                        newAssetPath = GetAssetPath(costume, AssetType.CharacterGfs_B, isTex);
                    }
                    break;
            }

            Log.Information($"Redirected Asset || {character} || Costume: {costume.Name} || {costume.CostumeId}");
        }

        if (!string.IsNullOrEmpty(newAssetPath))
        {
            Marshal.Copy(Encoding.ASCII.GetBytes(newAssetPath + "\0"), 0, buffer, newAssetPath.Length + 1);
        }
    }

    private static string? GetAssetPath(Costume costume, AssetType assetType, bool isTex)
        => assetType switch
        {
            AssetType.CharacterGfs_B => isTex ? costume.Config.GetAssetPath(CostumeAsset.CharBattleTex) : costume.Config.GetAssetPath(CostumeAsset.CharBattleGfs),
            AssetType.CharacterGfs_F_2 or AssetType.CharacterGfs_F_5 => isTex ? costume.Config.GetAssetPath(CostumeAsset.CharFieldTex) : costume.Config.GetAssetPath(CostumeAsset.CharFieldGfs),
            AssetType.CharacterGfs_E => isTex ? costume.Config.GetAssetPath(CostumeAsset.CharEventTex) : costume.Config.GetAssetPath(CostumeAsset.CharEventGfs),
            _ => throw new NotImplementedException(),
        };

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
