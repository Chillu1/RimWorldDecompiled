using UnityEngine;

namespace Verse
{
	public class ScatterableDef : Def
	{
		[NoTranslate]
		public string texturePath;

		public float minSize;

		public float maxSize;

		public float selectionWeight = 100f;

		[NoTranslate]
		public string scatterType = "";

		public Material mat;

		public override void PostLoad()
		{
			base.PostLoad();
			if (defName == "UnnamedDef")
			{
				defName = "Scatterable_" + texturePath;
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				mat = MaterialPool.MatFrom(texturePath, ShaderDatabase.Transparent);
			});
		}
	}
}
