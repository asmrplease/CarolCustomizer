using System;
using System.IO;
using System.Linq;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using MonoMod.Utils;
using Newtonsoft.Json;

namespace CarolCustomizer.Behaviors.Recipes;
internal static class RecipeLoader
{
    public static string[] GetRecipeFilePaths()
    {
        return 
            Directory.GetFiles(
                Constants.RecipeFolderPath, $"*",
                SearchOption.AllDirectories)
            .Select(x=> (ext: Path.GetExtension(x), path: x))
            .Where(tup => 
                tup.ext == Constants.JsonFileExtension ||
                tup.ext == Constants.PngFileExtension)
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
                results = PngMetadataUtil.GetMetadata(path, Constants.PNGChunkKeyword);
                if (results == "") Log.Warning("empty json!");
                break;
            default: 
                Log.Warning("tried to load a recipe with an unsupported extension");
                break;
        }
        
        return results;
    }

    public struct ValidationResults { public Recipe.Status Status; public RecipeDescriptor23 Recipe; }

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
        var results = new ValidationResults { Status = Recipe.Status.NoError, Recipe = null };
        Version version;

        try
        {
            version = JsonConvert.DeserializeObject<VersionedObject>(json)?.version;
            if (version is null) { Log.Warning($"Version deserialization failed: {json}"); }
            version ??= Constants.v100;
        }
        catch { version = Constants.v100; }
        try
        {
            //TODO: this is gonna get out of hand sooner or later
            switch (version)
            {
                case var x when x >= Constants.v230:
                    Log.Debug("VRF dectected recipe version as >= 2.3.0");
                    results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor23>(json);
                    break;
                case var x when x >= Constants.v220:
                    Log.Debug("VRF dectected recipe version as >= 2.2.0");
                    results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor22>(json)
                        .ToVersion230();
                    break;
                case var x when x > Constants.v200:
                    Log.Debug("VRF dectected recipe version as > 2.0.0");
                    results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor21>(json)
                        .ToVersion220()
                        .ToVersion230();
                    break;
                default:
                    Log.Debug("Legacy descriptor");
                    results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor20>(json)
                        .ToVersion210()
                        .ToVersion220()
                        .ToVersion230(); 
                    break;
            }
        }
        catch (Exception ex)
        {
            ex.LogDetailed();
            results.Status = Recipe.Status.InvalidJson;
            return results;
        }

        if (results.Recipe is null) { results.Status = Recipe.Status.InvalidJson; return results; }

        try
        {
            if (RecipeApplier.GetMissingSources(results.Recipe).Any())
            { results.Status = Recipe.Status.MissingSource; }
        }
        catch { results.Status = Recipe.Status.MissingSource; }

        return results;
    }
}