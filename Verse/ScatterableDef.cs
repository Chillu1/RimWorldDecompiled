using UnityEngine;

namespace Verse;

public class ScatterableDef : Def
{
	[NoTranslate]
	public string texturePath;

	public float minSize;

	public float maxSize;

	public float scatterChance = 0.5f;

	[NoTranslate]
	public string scatterType = "";

	public bool placeUnderNaturalRoofs = true;

	public Material mat;

	public override void PostLoad()
	{
		base.PostLoad();
		if (defName == "UnnamedDef")
		{
			defName = "Scatterable_" + texturePath;
			ResolveDefNameHash();
		}
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			GlobalTextureAtlasManager.TryInsertStatic(TextureAtlasGroup.Terrain, ContentFinder<Texture2D>.Get(texturePath));
		});
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			mat = MaterialPool.MatFrom(texturePath, ShaderDatabase.Transparent);
		});
	}
}
