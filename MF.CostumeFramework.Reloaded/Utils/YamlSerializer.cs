﻿using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace MF.CostumeFramework.Reloaded.Utils;

internal static class YamlSerializer
{
    private static readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    public static T DeserializeFile<T>(string file)
        => deserializer.Deserialize<T>(File.ReadAllText(file));

    public static void SerializeFile<T>(string file, T obj)
        => File.WriteAllText(file, serializer.Serialize(obj));
}
