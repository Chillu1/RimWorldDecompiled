using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class KnowledgeCategoryDef : Def
{
	public KnowledgeCategoryDef overflowCategory;

	public Color color;

	[NoTranslate]
	public string texPath;

	[Unsaved(false)]
	private Texture2D texInt;

	public Texture2D Tex => texInt;

	public override IEnumerable<string> ConfigErrors()
	{
		if (overflowCategory == this)
		{
			yield return "overflowCategory is this category.";
		}
	}

	public override void PostLoad()
	{
		if (!texPath.NullOrEmpty())
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				texInt = ContentFinder<Texture2D>.Get(texPath);
			});
		}
	}
}
