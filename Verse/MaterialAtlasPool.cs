using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public static class MaterialAtlasPool
{
	private class MaterialAtlas
	{
		protected Material[] subMats = new Material[16];

		private const float TexPadding = 1f / 32f;

		public MaterialAtlas(Material newRootMat)
		{
			Vector2 mainTextureScale = new Vector2(0.1875f, 0.1875f);
			for (int i = 0; i < 16; i++)
			{
				float x = (float)(i % 4) * 0.25f + 1f / 32f;
				float y = (float)(i / 4) * 0.25f + 1f / 32f;
				Vector2 mainTextureOffset = new Vector2(x, y);
				Material material = MaterialAllocator.Create(newRootMat);
				material.name = newRootMat.name + "_ASM" + i;
				material.mainTextureScale = mainTextureScale;
				material.mainTextureOffset = mainTextureOffset;
				subMats[i] = material;
			}
		}

		public Material SubMat(LinkDirections linkSet)
		{
			int num = (int)linkSet;
			if (num >= subMats.Length)
			{
				Log.Warning("Cannot get submat of index " + num + ": out of range.");
				return BaseContent.BadMat;
			}
			return subMats[num];
		}
	}

	private static Dictionary<Material, MaterialAtlas> atlasDict = new Dictionary<Material, MaterialAtlas>();

	public static Material SubMaterialFromAtlas(Material mat, LinkDirections LinkSet)
	{
		if (!atlasDict.ContainsKey(mat))
		{
			atlasDict.Add(mat, new MaterialAtlas(mat));
		}
		return atlasDict[mat].SubMat(LinkSet);
	}
}
