using UnityEngine;

namespace FG.Mods.YSYard.Translations.Helpers
{
    public static class Texture2DExtensions
	{
		public static Texture2D CloneReadable(this Texture2D source)
		{
			RenderTexture renderTexture = RenderTexture.GetTemporary(
				source.width,
				source.height,
				0,
				RenderTextureFormat.Default,
				RenderTextureReadWrite.Linear);

			Graphics.Blit(source, renderTexture);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = renderTexture;
			Texture2D readableTextur2D = new Texture2D(source.width, source.height);
			readableTextur2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			readableTextur2D.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTexture);
			return readableTextur2D;
		}
	}
}
