namespace MF.CostumeFramework.Reloaded.Costumes.Models;

internal class Costume
{
    private const string DEF_DESC = "Costume added with <COLOR 0>Costume Framework<COLOR -1>.";

    public Costume()
    {
    }

    public Costume(ushort costumeId)
    {
        this.CostumeId = costumeId;
    }

    public ushort CostumeId { get; }

    public int CostumeItemId { get; private set; }

    public string Name { get; set; } = "Missing Name";

    public string Description { get; set; } = DEF_DESC;

    public Character Character { get; set; }

    public bool IsEnabled { get; set; }

    public CostumeConfig Config { get; } = new();

    public string? OwnerModId { get; set; }

    public string? MusicScriptFile { get; set; }

    public string? BattleThemeFile { get; set; }

    public string? NameMsgLabel { get; set; }

    public string? DescMsgLabel { get; set; }

    /// <summary>
    /// Sets the costume item ID. Costume Item ID = Actual Item ID - Base Costume Item ID (0x6000).
    /// </summary>
    /// <param name="costumeItemId">Costume Item ID.</param>
    public void SetCostumeItemId(int costumeItemId) => this.CostumeItemId = costumeItemId;
}
