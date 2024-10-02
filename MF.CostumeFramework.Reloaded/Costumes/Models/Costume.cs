namespace MF.CostumeFramework.Reloaded.Costumes.Models;

internal class Costume
{
    private const string DEF_DESC = "Costume added with Costume Framework.";

    public Costume()
    {
    }

    public Costume(int costumeId)
    {
        this.CostumeId = costumeId;
    }

    public int CostumeId { get; }

    public string Name { get; set; } = "Missing Name";

    public string Description { get; set; } = DEF_DESC;

    public Character Character { get; set; }

    public bool IsEnabled { get; set; }

    public CostumeConfig Config { get; } = new();

    public string? OwnerModId { get; set; }

    public string? MusicScriptFile { get; set; }

    public string? BattleThemeFile { get; set; }
}
