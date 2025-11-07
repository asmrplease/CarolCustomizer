using System;
using System.IO;
using System.Linq;
using CarolCustomizer.Assets;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using MonoMod.Utils;
using static Newtonsoft.Json.JsonConvert;
using static CarolCustomizer.Utils.Constants;

namespace CarolCustomizer.Behaviors.Recipes;
internal static class RecipeLoader
{
    public static string[] GetRecipeFilePaths()
    {
        return
            Directory.GetFiles(
                RecipeFolderPath, $"*",
                SearchOption.AllDirectories)
            .Select(x => (ext: Path.GetExtension(x), path: x))
            .Where(tup =>
                tup.ext == JsonFileExtension ||
                tup.ext == PngFileExtension)
            .Select(tup => tup.path)
            .ToArray();
    }

    public static string GetRecipeJson(string path)
    {
        string results = "";

        switch (Path.GetExtension(path))
        {
            case ".json":
                var file = File.OpenText(path);
                if (file is null) { Log.Warning("failed to open file"); return ""; }

                results = file.ReadToEnd();
                file.Close();
                break;
            case ".png":
                results = PngMetadataUtil.GetMetadata(path, PNGChunkKeyword);
                if (results == "") Log.Warning("empty json!");
                break;
            default:
                Log.Warning("tried to load a recipe with an unsupported extension");
                break;
        }

        return results;
    }

    public struct ValidationResults 
    { 
        public Recipe.Status Status; 
        public RecipeDescriptor Recipe; 
        public string Json; 
    }

    public static ValidationResults ValidateRecipeFile(string filePath)
    {
        string json;
        var results = new ValidationResults { Status = Recipe.Status.NoError, Recipe = null };

        try { json = GetRecipeJson(filePath); }
        catch (Exception e)
        {
            Log.Error(e.StackTrace);
            results.Status = Recipe.Status.FileError; return results;
        }
        return ValidateJson(json);
    }

    public static ValidationResults ValidateJson(string json)
    {
        var results = new ValidationResults 
        { 
            Status = Recipe.Status.NoError, 
            Recipe = null,
            Json = json,
        };
        Version version;

        try
        {
            version = DeserializeObject<VersionedObject>(json)?.version;
            if (version is null) { Log.Warning($"Version deserialization failed: {json}"); }
            version ??= v100;
        }
        catch { version = v100; }
        try
        {
            Log.Debug($"VRF dectected recipe version as {version}");
            version = new Version(version.Major, version.Minor, 0);
            var v20 = version == v200 ? DeserializeObject<RecipeDescriptor20>(json) : null;
            var v21 = version == v210 ? DeserializeObject<RecipeDescriptor21>(json) : v20?.ToVersion210();
            var v22 = version == v220 ? DeserializeObject<RecipeDescriptor22>(json) : v21?.ToVersion220();
            var v23 = version == v230 ? DeserializeObject<RecipeDescriptor23>(json) : v22?.ToVersion230();
            var v24 = version == v240 ? DeserializeObject<RecipeDescriptor24>(json) : v23?.ToVersion240();
            results.Recipe = v24?.ToVersion250() ?? DeserializeObject<RecipeDescriptor25>(json);
        }
        catch (Exception ex)
        {
            ex.LogDetailed();
            results.Status = Recipe.Status.InvalidJson;
            return results;
        }

        if (results.Recipe is null) { results.Status = Recipe.Status.InvalidJson; return results; }

        bool slow = SceneResourceProvider
            .CheckMaterialsReady(RecipeApplier.GetWorldMats(results.Recipe))
            .Any();
        if (slow) results.Status = Recipe.Status.SlowSource;

        try
        {
            if (RecipeApplier.GetMissingSources(results.Recipe).Any())
            { results.Status = Recipe.Status.MissingSource; }
        }
        catch { results.Status = Recipe.Status.MissingSource; }
        return results;
    }
}