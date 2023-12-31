﻿using System.Text.Json.Serialization;

namespace Distribus.Files;

[JsonSourceGenerationOptions(
    IgnoreReadOnlyFields = true,
    IgnoreReadOnlyProperties = true,
    IncludeFields = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = false,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(FileIndex))]
public partial class FileIndexerSerializerContext : JsonSerializerContext
{
}