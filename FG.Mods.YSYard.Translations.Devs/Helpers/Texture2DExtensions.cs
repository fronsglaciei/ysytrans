using UnityEngine;

namespace FG.Mods.YSYard.Translations.Devs.Helpers;

public static class Texture2DExtensions
{
    public static Texture2D CloneReadable(this Texture2D source)
    {
        var renderTexture = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTexture);
        var previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        var readableTextur2D = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true);
        readableTextur2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        readableTextur2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readableTextur2D;
    }
}
