using MF.CostumeFramework.Reloaded.Costumes;
using MF.CostumeFramework.Reloaded.Costumes.Models;
using MF.Toolkit.Interfaces.Inventory;
using MF.Toolkit.Interfaces.Messages;
using Reloaded.Hooks.Definitions;
using System.Runtime.InteropServices;

namespace MF.CostumeFramework.Reloaded.Hooks;

internal unsafe class CostumeTblHooks
{
    private delegate CostumeTbl* PostLoadCostumeTbl(CostumeTbl* ogTbl);
    private IReverseWrapper<PostLoadCostumeTbl>? loadCostumeWrapper;
    private IAsmHook? loadCostumeAsm;

    private readonly IMessage _msg;
    private readonly IInventory _inv;
    private readonly CostumeRegistry _registry;

    public CostumeTblHooks(IMessage msg, IInventory inv, CostumeRegistry costumes)
    {
        _msg = msg;
        _inv = inv;
        _registry = costumes;

        ScanHooks.Add(
            "Load CostumeItem.TBL",
            "B9 00 00 03 00",
            (hooks, result) =>
            {
                var patch = new string[]
                {
                    "use64",
                    "mov rcx, rax",
                    Utilities.PushCallerRegisters,
                    hooks.Utilities.GetAbsoluteCallMnemonics(LoadCostume, out loadCostumeWrapper),
                    Utilities.PopCallerRegisters,
                };

                loadCostumeAsm = hooks.CreateAsmHook(patch, result).Activate();
            });
    }

    private CostumeTbl* LoadCostume(CostumeTbl* ogTbl)
    {
        var newTbl = new CostumeTbl();

        // Copy game costumes to new TBL.
        for (int i = 0; i < ogTbl->NumEntries; i++)
        {
            *newTbl.GetCostume(i) = *ogTbl->GetCostume(i);
            Log.Debug($"Copied Costume: {i}");
        }

        var addedCostumes = new HashSet<Costume>();
        for (int i = ogTbl->NumEntries; i < newTbl.NumEntries; i++)
        {
            var item = newTbl.GetCostume(i);
            *item = new();
            item->MsgSerial.UseCustomSerial();

            var newCostume = _registry.Costumes.FirstOrDefault(x => !addedCostumes.Contains(x));
            if (newCostume != null)
            {
                item->EquipFlag = GetEquippable(newCostume.Character);
                item->CostumeId = newCostume.CostumeId;
                item->Rarity = Rarity.Legendary;
                newCostume.SetCostumeItemId(i);
                
                if (newCostume.IsEnabled) _inv.UnlockItem(i + 0x6000);

                _msg.SetItemMessage(i + 0x6000, ItemMsg.Name, newCostume.ItemMessageLabel!);
                _msg.SetItemMessage(i + 0x6000, ItemMsg.Description, newCostume.ItemMessageLabel!);
                addedCostumes.Add(newCostume);
                Log.Debug($"Costume added for: {newCostume.Character} || Costume ID: {newCostume.CostumeId} || Costume Item ID: {newCostume.CostumeItemId}");
            }
        }

        newTbl.NumEntries = (ushort)(ogTbl->NumEntries + addedCostumes.Count);

        var newTblPtr = Marshal.AllocHGlobal(sizeof(CostumeTbl));
        Log.Debug($"New ItemCostume.TBL: 0x{newTblPtr:X}");
        Marshal.StructureToPtr(newTbl, newTblPtr, false);
        return (CostumeTbl*)newTblPtr;
    }

    private static GearEquippable GetEquippable(Character character) => Enum.Parse<GearEquippable>(character.ToString());
}

public unsafe struct CostumeTbl
{
    public const int NUM_COSTUMES = NUM_GAME_COSTUMES + NUM_MOD_COSTUMES;

    private const int NUM_GAME_COSTUMES = 92;
    private const int ALLOC_SIZE = 0x3000;
    private const int ITEM_SIZE = 0x30;
    private const int NUM_MOD_COSTUMES = (ALLOC_SIZE / ITEM_SIZE) - NUM_GAME_COSTUMES;

    public CostumeTbl()
    {
    }

    public ushort NumEntries = NUM_COSTUMES;
    public fixed byte Costumes[NUM_COSTUMES * ITEM_SIZE];

    public readonly CostumeItem* GetCostume(int idx)
    {
        if (idx >= NumEntries)
        {
            return null;
        }

        fixed (CostumeTbl* self = &this)
        {
            CostumeItem* costumes = (CostumeItem*)(self->Costumes);
            return &costumes[idx];
        }
    }
}


[StructLayout(LayoutKind.Sequential, Size = 0x30, Pack = 1)]
public unsafe struct CostumeItem
{
    public const int BASE_ITEM_ID = 0x6000; // Base + Index = Costume Item ID

    public CostumeItem()
    {
    }

    public uint Icon = 59;
    public GearEquippable EquipFlag;
    public ushort Field3;
    public Rarity Rarity = Rarity.Common;
    public ushort CostumeId;
    public ItemMsgSerial MsgSerial;
}

[Flags]
public enum GearEquippable
{
    NotEquippable = 0,
    AnyoneEquippable = -1,

    Player = 1 << 1,
    Prince = 1 << 1,
    Strohl = 1 << 2,
    Hulkenberg = 1 << 3,
    Heismay = 1 << 4,
    Junah = 1 << 5,
    Eupha = 1 << 6,
    Fiedlio = 1 << 7,
    Basilio = 1 << 8,
    Grius = 1 << 10,
    Gallica = 1 << 11,
    PX = 1 << 12,
    Neuras = 1 << 13,
    Rella = 1 << 14,
    Louis = 1 << 15,
}

public enum Rarity : ushort
{
    //None, // Might cause crashes.
    Common = 1,
    Uncommon,
    Rare,
    Legendary,
}