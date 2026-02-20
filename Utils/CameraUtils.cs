using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Utils;
public static class CameraUtils
{
    /// <summary>
    /// Takes a photo and uses color subtraction to produce alpha. 
    /// </summary>
    /// <param name="camera">Camera to use.</param>
    /// <param name="resolution">Output texture resolution.</param>
    /// <returns>Texure2D captured by camera with background alpha.</returns>
    public static Texture2D CaptureAlpha(this Camera camera, int resolution)
    {
        var black = camera.Capture(Color.black, resolution);
        var white = camera.Capture(Color.white, resolution);
        return CameraUtils.CalculateTransparency(black, white);
    }

    /// <summary>
    /// Enables the camera, takes a photo, then disables the camera. 
    /// </summary>
    /// <param name="camera">Camera to use.</param>
    /// <param name="bgColor">Background color to use in place of transparent pixels.</param>
    /// <param name="resolution">Output texture resolution.</param>
    /// <returns>Texure2D captured by camera with the specified background color.</returns>
    public static Texture2D Capture(this Camera camera, Color bgColor, int resolution)
    {
        camera.enabled = true;
        RenderTexture.active =
            camera.targetTexture
            ??= new(resolution, resolution, 32);
        camera.backgroundColor = bgColor;
        camera.Render();
        Log.Debug("Render Complete");
        var result = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        if (result) Log.Debug("created texture2d");

        result.ReadPixels(
            new Rect(
                0, 0,
                camera.targetTexture.width,
                camera.targetTexture.height),
            0, 0);

        result.Apply();
        Log.Debug("ReadPixels complete");
        camera.enabled = false;
        return result;
    }

    /// <summary>
    /// Uses pixel subtraction to produce an image with transparency given two images with 
    /// </summary>
    /// <param name="blackTexture"></param>
    /// <param name="whiteTexture"></param>
    /// <returns></returns>
    public static Texture2D CalculateTransparency(Texture2D blackTexture, Texture2D whiteTexture)
    {
        const int mipmap = 0;
        var alphaTex = new Texture2D(
            blackTexture.width,
            blackTexture.height,
            TextureFormat.RGBA32, false);
        var black = blackTexture.GetPixelData<Color32>(mipmap);
        var white = whiteTexture.GetPixelData<Color32>(mipmap);
        var alpha = black
            .Zip(white, DifferenceToAlpha)
            .ToArray();
        alphaTex.SetPixelData(alpha, mipmap);
        return alphaTex;
    }

    public static Color32 DifferenceToAlpha(Color32 black, Color32 white)
    {
        const int alphaCutoff = 200;
        var result = new Color32(black.r, black.g, black.b, 0);
        var total = (3 * 255)
            - (white.r - black.r)
            - (white.g - black.g)
            - (white.b - black.b);
        //alpha 255 is opaque, 0 is transparent
        result.a = (byte)(total > alphaCutoff ? 255 : total >> 2);
        return result;
    }
}
