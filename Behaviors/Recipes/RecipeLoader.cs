using CarolCustomizer.Assets;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using MonoMod.Utils;
using PngHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static CarolCustomizer.Utils.Constants;
using static Newtonsoft.Json.JsonConvert;

namespace CarolCustomizer.Behaviors.Recipes;
public static class RecipeLoader
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

    public static (string, RichPng) GetRecipeJson(string path)
    {
        string results = "";
        RichPng png = null;

        switch (Path.GetExtension(path))
        {
            case ".json":
                var file = File.OpenText(path);
                if (file is null) { Log.Warning("failed to open file"); return ("", null); }

                results = file.ReadToEnd();
                file.Close();
                break;
            case ".png":
                png = new RichPng(path);
                png.Keywords.TryGetValue(Constants.PNGChunkKeyword, out results);
                break;
            default:
                Log.Warning("tried to load a recipe with an unsupported extension");
                break;
        }

        return (results, png);
    }

    public struct ValidationResults 
    { 
        public Recipe.Status Status; 
        public RecipeDescriptor Recipe; 
        public string Json;
        public IEnumerable<AccessoryDescriptor> MissingAccs;
        public IEnumerable<SourceDescriptor> MissingSources;
        public RichPng Png;
    }

    public static (bool, RecipeDescriptor) Deserialize(string json)
    {
        Version version;
        RecipeDescriptor rd;
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
            rd = v24?.ToVersion250() ?? DeserializeObject<RecipeDescriptor25>(json);
        }
        catch (Exception ex)
        {
            ex.LogDetailed();
            return (false, null);
        }
        return (true, rd);
    }

    public static ValidationResults ValidateRecipeFile(string filePath)
    {
        Log.Debug($"ValidateRecipeFile({filePath})");
        (string, RichPng) idk;
        var results = new ValidationResults 
        { 
            Status = Recipe.Status.NoError, 
            Recipe = null, 
            MissingAccs = [], 
            MissingSources = [] 
        };

        try { idk = GetRecipeJson(filePath); }
        catch (Exception e)
        {
            Log.Error(e.StackTrace);
            results.Status = Recipe.Status.FileError; return results;
        }
        var json = idk.Item1 ??= "";
        if (idk.Item2 is not null) results.Png = idk.Item2;
        json = json.Trim();
        var tup = Deserialize(json);
        if (tup.Item1) results.Recipe = tup.Item2;
        else {results.Status = Recipe.Status.InvalidJson; return results; }

        bool slow = SceneResourceProvider
            .CheckMaterialsReady(RecipeApplier.GetWorldMats(results.Recipe))
            .Any();
        if (slow) results.Status = Recipe.Status.SlowSource;

        try
        {
            results.MissingSources = RecipeApplier.GetMissingSources(results.Recipe);
            if (results.MissingSources.Any())
            { results.Status = Recipe.Status.Incomplete; }

            results.MissingAccs = RecipeApplier.GetRemovedAccessories(results.Recipe);
            if (results.MissingAccs.Any())
            { results.Status |= Recipe.Status.Incomplete; }
        }
        catch { results.Status = Recipe.Status.Incomplete; }
        return results;
    }
}