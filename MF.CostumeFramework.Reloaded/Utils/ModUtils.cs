namespace MF.CostumeFramework.Reloaded.Utils;

internal static class ModUtils
{
    public static void IfNotNull<T>(T obj, Action<T> action)
    {
        if (obj != null) action(obj);
    }
}
