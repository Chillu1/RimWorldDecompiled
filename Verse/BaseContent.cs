using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class BaseContent
	{
		public static readonly string BadTexPath = "UI/Misc/BadTexture";

		public static readonly string PlaceholderImagePath = "PlaceholderImage";

		public static readonly Material BadMat = MaterialPool.MatFrom(BadTexPath, ShaderDatabase.Cutout);

		public static readonly Texture2D BadTex = ContentFinder<Texture2D>.Get(BadTexPath);

		public static readonly Graphic BadGraphic = GraphicDatabase.Get<Graphic_Single>(BadTexPath);

		public static readonly Texture2D BlackTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

		public static readonly Texture2D GreyTex = SolidColorMaterials.NewSolidColorTexture(Color.grey);

		public static readonly Texture2D WhiteTex = SolidColorMaterials.NewSolidColorTexture(Color.white);

		public static readonly Texture2D ClearTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		public static readonly Texture2D YellowTex = SolidColorMaterials.NewSolidColorTexture(Color.yellow);

		public static readonly Material BlackMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.black);

		public static readonly Material WhiteMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.white);

		public static readonly Material ClearMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.clear);

		public static bool NullOrBad(this Material mat)
		{
			if (!(mat == null))
			{
				return mat == BadMat;
			}
			return true;
		}

		public static bool NullOrBad(this Texture2D tex)
		{
			if (!(tex == null))
			{
				return tex == BadTex;
			}
			return true;
		}
	}
}
