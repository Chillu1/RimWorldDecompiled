using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class InvisibilityMatPool
	{
		private static Dictionary<Material, Material> materials = new Dictionary<Material, Material>();

		private static Color color = new Color(0.75f, 0.93f, 0.98f, 0.5f);

		private static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");

		public static Material GetInvisibleMat(Material baseMat)
		{
			if (!materials.TryGetValue(baseMat, out var value))
			{
				value = MaterialAllocator.Create(baseMat);
				value.shader = ShaderDatabase.Invisible;
				value.SetTexture(NoiseTex, TexGame.InvisDistortion);
				value.color = color;
				materials.Add(baseMat, value);
			}
			return value;
		}
	}
}
