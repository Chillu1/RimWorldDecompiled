using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class StyleItemDef : Def
{
	[LoadAlias("hairTags")]
	[NoTranslate]
	public List<string> styleTags = new List<string>();

	[LoadAlias("hairGender")]
	public StyleGender styleGender = StyleGender.Any;

	private StyleItemCategoryDef category;

	public bool noGraphic;

	public ShaderTypeDef overrideShaderTypeDef;

	public GeneDef requiredGene;

	public MutantDef requiredMutant;

	[NoTranslate]
	public string texPath;

	[NoTranslate]
	public string iconPath;

	public virtual Texture2D Icon
	{
		get
		{
			if (noGraphic)
			{
				return BaseContent.ClearTex;
			}
			return ContentFinder<Texture2D>.Get(iconPath ?? (texPath + "_south"));
		}
	}

	public StyleItemCategoryDef StyleItemCategory
	{
		get
		{
			if (category == null)
			{
				return StyleItemCategoryDefOf.Misc;
			}
			return category;
		}
	}

	public static IEnumerable<StyleItemDef> AllStyleItemDefs
	{
		get
		{
			foreach (HairDef allDef in DefDatabase<HairDef>.AllDefs)
			{
				yield return allDef;
			}
			foreach (BeardDef allDef2 in DefDatabase<BeardDef>.AllDefs)
			{
				yield return allDef2;
			}
			foreach (TattooDef allDef3 in DefDatabase<TattooDef>.AllDefs)
			{
				yield return allDef3;
			}
		}
	}

	public abstract Graphic GraphicFor(Pawn pawn, Color color);

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (texPath.NullOrEmpty() && !noGraphic)
		{
			yield return "no texPath with noGraphic disabled.";
		}
	}
}
