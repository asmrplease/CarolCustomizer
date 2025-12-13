using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using PngHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Recipes;
public class PngUtil
{
    public static void BuildPng(string file, int resolution, byte[] imageBytes, List<(string, string)> kvps)
    {
        //open file
        Log.Debug("BuildPng()");
        using var stream = File.Open(file, FileMode.Create);
        var builder = new PngBuilder()
        {
            width = (uint) resolution,
            height = (uint) resolution,
            rawData = imageBytes,
        };
        builder.textKeys.AddRange(kvps);
        //build png
        builder.Save(stream);
        Log.Info("BuildPng Completed");
    }

    public static byte[] RebuildPng(IDimension dimensions, byte[] compressedBytes)
    {
        var builder = new PngBuilder()
        {
            width = dimensions.Width,
            height = dimensions.Height,
        };

        builder.compressedData = compressedBytes;
        using var stream = new MemoryStream();
        builder.Save(stream);
        //string path = @"D:\Rendered\apng\engine.png";
        //using var file = File.Create(path);
        var array = stream.ToArray();
        //file.Write(array);
        return array;   
    }

    public static Sprite BuildSprite(RichPng png)
    {
        var (dimensions, compressedData) = png.CompressedFrames[0];
        return PngUtil.BuildSprite(dimensions, compressedData);
    }

    public static Sprite BuildSprite(IDimension dimensions, byte[] compressedData)
    {
        var width = (int)dimensions.Width;
        var height = (int)dimensions.Height;
        Texture2D thumbnail = new(width, height, TextureFormat.RGBA32, false);
        var png = PngUtil.RebuildPng(dimensions, compressedData);
        ImageConversion.LoadImage(thumbnail, png);
        //var inflated = Compressor.Inflate(compressedData);
        //var raw = PixelFormatter.ScanlinesToRaw(inflated, width, height);
        //thumbnail.LoadRawTextureData(raw);
        //thumbnail.Apply();
        return Sprite.Create
        (
            thumbnail,
            new Rect(0, 0, thumbnail.width, thumbnail.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    public static List<Recipe> ReadRecipes(RichPng png)
    {
        var frames = png.CompressedFrames
            .Select(tup => BuildSprite(tup.Item1, tup.Item2));
        var results = png.Keywords
            .Where(kvp => kvp.Key.Contains("RecipeData"))
            .Select(kvp => (name: kvp.Key.Replace("Data", ""), recipe: RecipeLoader.Deserialize(kvp.Value).Item2))
            .Zip(frames, (tup, frame) => new Recipe(tup.name, tup.recipe, frame))
            .ToList();
        return results;
    }

    //public static void AddMetadata(string origFilename, string key, string value)
    //{
    //    string tempFileName = "tmp.png";
    //    using var stream = File.OpenRead(origFilename);
    //    //create png builder
    //    //

    //    PngWriter writer = FileHelper.CreatePngWriter(tempFileName, reader.ImgInfo, true);
    //    writer.CopyChunksFirst(reader, chunkBehav);
    //    Hjg.Pngcs.Chunks.PngMetadata metadata = writer.GetMetadata();
    //    data
    //        .Select(kvp => metadata.SetText(kvp.Key, kvp.Value))
    //        .ForEach(chunk => chunk.Priority = true);
    //    Enumerable
    //        .Range(0, reader.ImgInfo.Rows)
    //        .ForEach(i => writer.WriteRow(reader.ReadRowInt(i), i));
    //    writer.CopyChunksLast(reader, chunkBehav);
    //    writer.End();
    //    reader.End();
    //    File.Delete(origFilename);
    //    File.Move(tempFileName, origFilename);
    //    return true;
    //}

}
