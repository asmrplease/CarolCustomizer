using System.Collections.Generic;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using System.IO;
using CarolCustomizer.Utils;
using System.Linq;

namespace CarolCustomizer.Behaviors.Recipes;
public class PngMetadataUtil
{
    public static string GetMetadata(string file, string key)
    {
        PngReader reader = FileHelper.CreatePngReader(file);
        if (reader is null) Log.Warning($"Failed to create PNG reader for {file}");

        string data = reader
            .GetMetadata()
            .GetTxtForKey(key);
        reader.End();
        return data;
    }

    public static void AddMetadata(string origFilename, string key, string value)
    {
        AddMetadata(
            origFilename, 
            new Dictionary<string, string> { { key, value } });
    }

    public static bool AddMetadata(string origFilename, Dictionary<string, string> data)
    {
        string tempFileName = "tmp.png";
        var chunkBehav = ChunkCopyBehaviour.COPY_ALL_SAFE;
        PngReader reader = FileHelper.CreatePngReader(origFilename);
        if (reader.ImgInfo.Channels < 3) 
        { 
            Log.Error("Failed to write to png due to unexpected color channel count");
            reader.End();
            return false; 
        }

        PngWriter writer = FileHelper.CreatePngWriter(tempFileName, reader.ImgInfo, true); 
        writer.CopyChunksFirst(reader, chunkBehav);
        Hjg.Pngcs.Chunks.PngMetadata metadata = writer.GetMetadata();
        data
            .Select(kvp => metadata.SetText(kvp.Key, kvp.Value))
            .ForEach(chunk => chunk.Priority = true);
        Enumerable
            .Range(0, reader.ImgInfo.Rows)
            .ForEach(i => writer.WriteRow(reader.ReadRowInt(i), i));
        writer.CopyChunksLast(reader, chunkBehav);
        writer.End(); 
        reader.End();
        File.Delete(origFilename);
        File.Move(tempFileName, origFilename);
        return true;
    }
}
