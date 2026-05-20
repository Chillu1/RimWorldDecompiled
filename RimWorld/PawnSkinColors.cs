using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PawnSkinColors
{
	private static List<GeneDef> skinColorGenes;

	private static List<GeneDef> tmpSkinColorGenes = new List<GeneDef>();

	public static List<GeneDef> SkinColorGenesInOrder
	{
		get
		{
			if (skinColorGenes == null)
			{
				skinColorGenes = new List<GeneDef>();
				foreach (GeneDef allDef in DefDatabase<GeneDef>.AllDefs)
				{
					if ((allDef.endogeneCategory == EndogeneCategory.Melanin || !(allDef.minMelanin >= 0f)) && allDef.skinColorBase.HasValue)
					{
						skinColorGenes.Add(allDef);
					}
				}
				skinColorGenes.SortBy((GeneDef x) => x.minMelanin);
			}
			return skinColorGenes;
		}
	}

	public static void ResetStaticData()
	{
		skinColorGenes = null;
	}

	public static bool IsDarkSkin(Color color)
	{
		Color skinColor = GetSkinColor(0.5f);
		return color.r + color.g + color.b <= skinColor.r + skinColor.g + skinColor.b + 0.01f;
	}

	public static Color GetSkinColor(float melanin)
	{
		return GetSkinColorGene(melanin).skinColorBase.Value;
	}

	public static GeneDef GetSkinColorGene(float melanin)
	{
		List<GeneDef> skinColorGenesInOrder = SkinColorGenesInOrder;
		for (int num = skinColorGenesInOrder.Count - 1; num >= 0; num--)
		{
			if (melanin >= skinColorGenesInOrder[num].minMelanin)
			{
				return skinColorGenesInOrder[num];
			}
		}
		return skinColorGenesInOrder.RandomElement();
	}

	public static GeneDef RandomSkinColorGene(Pawn pawn)
	{
		if (pawn.Faction != null)
		{
			return GetSkinColorGene(pawn.Faction.def.melaninRange.RandomInRange);
		}
		return SkinColorGenesInOrder.RandomElementByWeight((GeneDef x) => x.selectionWeight);
	}

	public static int SkinColorIndex(Pawn pawn)
	{
		if (pawn?.genes == null)
		{
			return -1;
		}
		GeneDef firstEndogeneByCategory = pawn.genes.GetFirstEndogeneByCategory(EndogeneCategory.Melanin);
		if (firstEndogeneByCategory != null)
		{
			return SkinColorGenesInOrder.IndexOf(firstEndogeneByCategory);
		}
		return -1;
	}

	public static List<GeneDef> SkinColorsFromParents(Pawn father, Pawn mother)
	{
		tmpSkinColorGenes.Clear();
		if (father == null && mother == null)
		{
			return tmpSkinColorGenes;
		}
		int num = SkinColorIndex(father);
		int num2 = SkinColorIndex(mother);
		if (num >= 0 && num2 < 0)
		{
			tmpSkinColorGenes.Add(father.genes.GetFirstEndogeneByCategory(EndogeneCategory.Melanin));
		}
		else if (num2 >= 0 && num < 0)
		{
			tmpSkinColorGenes.Add(mother.genes.GetFirstEndogeneByCategory(EndogeneCategory.Melanin));
		}
		else if (num >= 0 && num2 >= 0)
		{
			int num3 = Mathf.Min(num, num2);
			int num4 = Mathf.Max(num, num2);
			for (int i = num3; i <= num4; i++)
			{
				tmpSkinColorGenes.Add(SkinColorGenesInOrder[i]);
			}
		}
		return tmpSkinColorGenes;
	}
}
