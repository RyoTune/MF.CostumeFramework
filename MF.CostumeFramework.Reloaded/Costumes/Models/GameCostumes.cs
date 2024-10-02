using System.Collections;

namespace MF.CostumeFramework.Reloaded.Costumes.Models;

internal class GameCostumes : IReadOnlyList<Costume>
{
    public const ushort BASE_MOD_COSTUME_ID = 1000;
    public const ushort RANDOMIZED_COSTUME_ID = 10001;
    public const ushort NUM_GAME_COSTUMES = 92;
    public const ushort NUM_MOD_COSTUMES = 1000;

    private readonly List<Costume> _costumes = [];
    private readonly List<Costume> _modCostumes = [];

    public GameCostumes()
    {
        //for (int i = 0; i < NUM_GAME_COSTUMES ; i++)
        //{
        //    _costumes.Add(new(i));
        //}

        for (int i = 0; i < NUM_MOD_COSTUMES; i++)
        {
            var costume = new Costume((ushort)(BASE_MOD_COSTUME_ID + i));
            _costumes.Add(costume);
            _modCostumes.Add(costume);
        }
    }

    public Costume? GetNewCostume()
    {
        var newCostume = _modCostumes.FirstOrDefault();
        if (newCostume != null)
        {
            _modCostumes.Remove(newCostume);
        }

        return newCostume;
    }

    public Costume this[int index] => _costumes[index];
    public int Count => _costumes.Count;
    public IEnumerator<Costume> GetEnumerator() => _costumes.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _costumes.GetEnumerator();
}
