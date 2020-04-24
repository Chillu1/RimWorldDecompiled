using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class InvisibilityMatPool
	{
		private static Dictionary<Material, Material> materials = new Dictionary<Material, Material>();

		private static Color color = new Color(0.1f, 0.65f, 0.8f, 0.5f);

		private static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");

		public static Material GetInvisibleMat(Material baseMat)
		{
			if (!materials.TryGetValue(baseMat, out Material value))
			{
				value = MaterialAllocator.Create(baseMat);
				value.shader = ShaderDatabase.Invisible;
				value.SetTexture(NoiseTex, TexGame.RippleTex);
				value.color = color;
				materials.Add(baseMat, value);
			}
			return value;
		}
	}
}
