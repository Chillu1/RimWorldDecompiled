using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class CreepJoinerFormKindDef : PawnKindDef, ICreepJoinerDef
{
	private float weight = 1f;

	private float minCombatPoints;

	private bool canOccurRandomly = true;

	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterPrompt;

	public List<BodyTypeGraphicData> bodyTypeGraphicPaths = new List<BodyTypeGraphicData>();

	public List<HeadTypeDef> forcedHeadTypes;

	public TagFilter hairTagFilter;

	public TagFilter beardTagFilter;

	public Color? hairColorOverride;

	private List<CreepJoinerBaseDef> excludes = new List<CreepJoinerBaseDef>();

	private List<CreepJoinerBaseDef> requires = new List<CreepJoinerBaseDef>();

	public float Weight => weight;

	public List<CreepJoinerBaseDef> Excludes => excludes;

	public List<CreepJoinerBaseDef> Requires => requires;

	public float MinCombatPoints => minCombatPoints;

	public bool CanOccurRandomly => canOccurRandomly;

	public string GetBodyGraphicPath(Pawn pawn)
	{
		for (int i = 0; i < bodyTypeGraphicPaths.Count; i++)
		{
			if (bodyTypeGraphicPaths[i].bodyType == pawn.story.bodyType)
			{
				return bodyTypeGraphicPaths[i].texturePath;
			}
		}
		return null;
	}

	public bool StyleItemAllowed(StyleItemDef styleItem)
	{
		if (!ModLister.AnomalyInstalled)
		{
			return true;
		}
		bool flag = styleItem is HairDef;
		bool flag2 = styleItem is BeardDef;
		if (!flag && !flag2)
		{
			return true;
		}
		if (flag)
		{
			if (hairTagFilter != null && !hairTagFilter.Allows(styleItem.styleTags))
			{
				return false;
			}
		}
		else if (flag2 && beardTagFilter != null && !beardTagFilter.Allows(styleItem.styleTags))
		{
			return false;
		}
		return true;
	}
}
