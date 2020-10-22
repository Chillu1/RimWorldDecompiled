using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class TexGame
	{
		public static readonly Texture2D AlphaAddTex;

		public static readonly Texture2D RippleTex;

		public static readonly Texture2D NoiseTex;

		public static readonly Texture2D InvisDistortion;

		static TexGame()
		{
			AlphaAddTex = ContentFinder<Texture2D>.Get("Other/RoughAlphaAdd");
			RippleTex = ContentFinder<Texture2D>.Get("Other/Ripples");
			NoiseTex = ContentFinder<Texture2D>.Get("Other/Noise");
			InvisDistortion = ContentFinder<Texture2D>.Get("Other/InvisDistortion");
			Shader.SetGlobalTexture("_NoiseTex", NoiseTex);
			Shader.SetGlobalTexture("_RippleTex", RippleTex);
		}
	}
}
