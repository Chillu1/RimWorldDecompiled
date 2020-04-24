using UnityEngine;

namespace Verse
{
	public static class MaterialUtility
	{
		public static Texture2D GetMaskTexture(this Material mat)
		{
			if (!mat.HasProperty(ShaderPropertyIDs.MaskTex))
			{
				return null;
			}
			return (Texture2D)mat.GetTexture(ShaderPropertyIDs.MaskTex);
		}

		public static Color GetColorTwo(this Material mat)
		{
			if (!mat.HasProperty(ShaderPropertyIDs.ColorTwo))
			{
				return Color.white;
			}
			return mat.GetColor(ShaderPropertyIDs.ColorTwo);
		}
	}
}
