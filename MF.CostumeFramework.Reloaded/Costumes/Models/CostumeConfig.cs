namespace MF.CostumeFramework.Reloaded.Costumes.Models;

internal class CostumeConfig
{
    public string? Name { get; set; }

    public Model Battle { get; } = new();

    public Model Field { get; } = new();

    public Model Event { get; } = new();
}

internal class Model
{
    public string? GfsPath { get; set; }

    public string? TexPath { get; set; }
}
