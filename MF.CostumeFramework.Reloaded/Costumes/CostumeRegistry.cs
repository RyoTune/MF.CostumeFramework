using CriFs.V2.Hook.Interfaces;
using MF.CostumeFramework.Reloaded.Costumes.Models;
using System.Diagnostics.CodeAnalysis;

namespace MF.CostumeFramework.Reloaded.Costumes;

internal class CostumeRegistry
{
    private readonly GameCostumes costumes;
    private readonly CostumeFactory costumeFactory;

    public CostumeRegistry(ICriFsRedirectorApi criFsApi)
    {
        this.costumes = new GameCostumes();
        this.costumeFactory = new CostumeFactory(criFsApi, this.costumes);
    }

    public GameCostumes Costumes => this.costumes;

    public bool TryGetCostumeByItemId(Character character, int costumeItemId, [NotNullWhen(true)] out Costume? costume)
    {
        costume = this.costumes.FirstOrDefault(x => x.Character == character && x.CostumeItemId == costumeItemId);
        return costume != null;
    }

    public void AddCostumesFolder(string modId, string costumesDir)
    {
        var ctx = new CostumeContext(modId, costumesDir);
        foreach (var character in Enum.GetValues<Character>())
        {
            var charDir = Path.Join(costumesDir, character.ToString());
            if (!Directory.Exists(charDir))
            {
                continue;
            }

            foreach (var costumeDir in Directory.EnumerateDirectories(charDir))
            {
                try
                {
                    this.costumeFactory.Create(ctx, character, costumeDir);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to create costume from folder.\nFolder: {costumeDir}");
                }
            }
        }
    }
}
