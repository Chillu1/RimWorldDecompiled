using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class MaterialLoader
	{
		public static List<Material> MatsFromTexturesInFolder(string dirPath)
		{
			return (from Texture2D tex in Resources.LoadAll("Textures/" + dirPath, typeof(Texture2D))
				select MaterialPool.MatFrom(tex)).ToList();
		}

		public static Material MatWithEnding(string dirPath, string ending)
		{
			Material material = (from mat in MatsFromTexturesInFolder(dirPath)
				where mat.mainTexture.name.ToLower().EndsWith(ending)
				select mat).FirstOrDefault();
			if (material == null)
			{
				Log.Warning("MatWithEnding: Dir " + dirPath + " lacks texture ending in " + ending);
				return BaseContent.BadMat;
			}
			return material;
		}
	}
}
