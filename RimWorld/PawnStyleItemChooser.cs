using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PawnStyleItemChooser
{
	public const float StyleItemFrequencyNever = 0f;

	public const float StyleItemFrequencyRare = 0.1f;

	public const float StyleItemFrequencyUncommon = 0.5f;

	public const float StyleItemFrequencyNormal = 1f;

	public const float StyleItemFrequencyCommon = 2f;

	public const float StyleItemFrequencyFrequent = 10f;

	private static List<StyleItemDef> tmpStyleItemDefs = new List<StyleItemDef>();

	private static List<StyleItemTagWeighted> tmpStyleItemTags = new List<StyleItemTagWeighted>();

	public static HairDef RandomHairFor(Pawn pawn)
	{
		if (pawn.kindDef.forcedHair != null)
		{
			return pawn.kindDef.forcedHair;
		}
		return ChooseStyleItem(pawn, HairDefOf.Bald);
	}

	public static BeardDef RandomBeardFor(Pawn pawn)
	{
		if (!pawn.style.CanWantBeard)
		{
			return BeardDefOf.NoBeard;
		}
		return ChooseStyleItem(pawn, BeardDefOf.NoBeard);
	}

	public static TattooDef RandomTattooFor(Pawn pawn, TattooType tattooType)
	{
		TattooDef tattooDef = ((tattooType == TattooType.Face) ? TattooDefOf.NoTattoo_Face : TattooDefOf.NoTattoo_Body);
		if (!ModsConfig.IdeologyActive)
		{
			return tattooDef;
		}
		if (pawn.Ideo == null && Rand.Chance(0.9f))
		{
			return tattooDef;
		}
		if (pawn.Ideo == null && Rand.Chance(0.9f))
		{
			return tattooDef;
		}
		return ChooseStyleItem(pawn, tattooDef, tattooType);
	}

	private static T ChooseStyleItem<T>(Pawn pawn, T fallback, TattooType? tattooType = null) where T : StyleItemDef
	{
		tmpStyleItemDefs.Clear();
		foreach (T allDef in DefDatabase<T>.AllDefs)
		{
			if (WantsToUseStyle(pawn, allDef, tattooType))
			{
				tmpStyleItemDefs.Add(allDef);
			}
		}
		if (tmpStyleItemDefs.TryRandomElementByWeight((StyleItemDef s) => TotalStyleItemLikelihood(s, pawn), out var result))
		{
			return (T)result;
		}
		return fallback;
	}

	public static bool WantsToUseStyle(Pawn pawn, StyleItemDef styleItemDef, TattooType? tattooType = null)
	{
		if (pawn.genes != null && ModsConfig.BiotechActive && !pawn.genes.StyleItemAllowed(styleItemDef))
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && pawn.IsMutant && !pawn.mutant.Def.StyleItemAllowed(styleItemDef))
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && pawn.IsCreepJoiner && !pawn.creepjoiner.form.StyleItemAllowed(styleItemDef))
		{
			return false;
		}
		if (styleItemDef is TattooDef tattooDef)
		{
			if (!ModLister.CheckIdeology("Tattoos"))
			{
				return false;
			}
			if (tattooType.HasValue && tattooDef.tattooType != tattooType.Value)
			{
				return false;
			}
		}
		else if (styleItemDef is BeardDef)
		{
			if (!pawn.style.CanWantBeard)
			{
				return styleItemDef == BeardDefOf.NoBeard;
			}
		}
		else if (styleItemDef is HairDef hair && !AgeAppropriateHairStyle(pawn, hair))
		{
			return false;
		}
		if (pawn.Ideo == null || Find.IdeoManager.classicMode)
		{
			return true;
		}
		if (pawn.Ideo.style.GetFrequency(styleItemDef) != StyleItemFrequency.Never)
		{
			return true;
		}
		if (pawn.genes != null && ModsConfig.BiotechActive && pawn.genes.StyleItemAllowed(styleItemDef))
		{
			return true;
		}
		return false;
	}

	public static bool AgeAppropriateHairStyle(Pawn pawn, HairDef hair)
	{
		LifeStageDef curLifeStage = pawn.ageTracker.CurLifeStage;
		if (curLifeStage.hairStyleFilter != null && !curLifeStage.hairStyleFilter.Allows(hair.styleTags))
		{
			return false;
		}
		return true;
	}

	private static float FrequencyFromPawnIdentity(Pawn pawn, StyleItemDef styleItem)
	{
		if (!pawn.kindDef.styleItemTags.NullOrEmpty())
		{
			return StyleItemChoiceLikelihoodFromTags(styleItem, pawn.kindDef.styleItemTags);
		}
		if (ModsConfig.AnomalyActive && pawn.IsMutant && pawn.mutant.Def.hairTagFilter != null)
		{
			return 1f;
		}
		if (!ModsConfig.IdeologyActive || Find.IdeoManager.classicMode)
		{
			tmpStyleItemTags.Clear();
			if (pawn.HomeFaction == null || pawn.HomeFaction.def.allowedCultures.NullOrEmpty())
			{
				return 1f;
			}
			foreach (CultureDef allowedCulture in pawn.HomeFaction.def.allowedCultures)
			{
				if (!allowedCulture.styleItemTags.NullOrEmpty())
				{
					tmpStyleItemTags.AddRange(allowedCulture.styleItemTags);
				}
			}
			if (!tmpStyleItemTags.Any())
			{
				return 1f;
			}
			return StyleItemChoiceLikelihoodFromTags(styleItem, tmpStyleItemTags);
		}
		if (pawn.Ideo == null)
		{
			return 1f;
		}
		return pawn.Ideo.style.GetFrequency(styleItem).GetFrequencyFloat();
	}

	public static float StyleItemChoiceLikelihoodFromTags(StyleItemDef styleItem, List<StyleItemTagWeighted> tags)
	{
		StyleItemTagWeighted styleItemTagWeighted = new StyleItemTagWeighted("", 0f);
		int i;
		for (i = 0; i < styleItem.styleTags.Count; i++)
		{
			StyleItemTagWeighted styleItemTagWeighted2 = tags.Find((StyleItemTagWeighted x) => x.Tag == styleItem.styleTags[i]);
			if (styleItemTagWeighted2 != null)
			{
				styleItemTagWeighted.Add(styleItemTagWeighted2);
			}
		}
		return Mathf.Max(0f, styleItemTagWeighted.TotalWeight);
	}

	public static float GetFrequencyFloat(this StyleItemFrequency styleItemFrequency)
	{
		switch (styleItemFrequency)
		{
		case StyleItemFrequency.Never:
			return 0f;
		case StyleItemFrequency.Rare:
			return 0.1f;
		case StyleItemFrequency.Uncommon:
			return 0.5f;
		case StyleItemFrequency.Normal:
			return 1f;
		case StyleItemFrequency.Common:
			return 2f;
		case StyleItemFrequency.Frequent:
			return 10f;
		default:
			Log.Error("Unknown StyleItemFrequency value.");
			return 0f;
		}
	}

	public static string GetLabel(this StyleItemFrequency styleItemFrequency)
	{
		return styleItemFrequency switch
		{
			StyleItemFrequency.Never => "StyleItemFrequencyNever".Translate(), 
			StyleItemFrequency.Rare => "StyleItemFrequencyRare".Translate(), 
			StyleItemFrequency.Uncommon => "StyleItemFrequencyUncommon".Translate(), 
			StyleItemFrequency.Normal => "StyleItemFrequencyNormal".Translate(), 
			StyleItemFrequency.Common => "StyleItemFrequencyCommon".Translate(), 
			StyleItemFrequency.Frequent => "StyleItemFrequencyFrequent".Translate(), 
			_ => "Unknown label", 
		};
	}

	public static StyleItemFrequency GetStyleItemFrequency(float freq)
	{
		if (freq <= 0f)
		{
			return StyleItemFrequency.Never;
		}
		if (freq <= 0.1f)
		{
			return StyleItemFrequency.Rare;
		}
		if (freq <= 0.5f)
		{
			return StyleItemFrequency.Uncommon;
		}
		if (freq <= 1f)
		{
			return StyleItemFrequency.Normal;
		}
		if (freq <= 2f)
		{
			return StyleItemFrequency.Common;
		}
		return StyleItemFrequency.Frequent;
	}

	public static float TotalStyleItemLikelihood(StyleItemDef styleItem, Pawn pawn)
	{
		return FrequencyFromGender(styleItem, pawn) * FrequencyFromPawnIdentity(pawn, styleItem);
	}

	public static float FrequencyFromGender(StyleItemDef styleItem, Pawn pawn)
	{
		if (ModsConfig.AnomalyActive && styleItem.requiredMutant != null && pawn.mutant?.Def != styleItem.requiredMutant)
		{
			return 0f;
		}
		if (pawn.gender == Gender.None || pawn.Ideo == null)
		{
			return 100f;
		}
		if (ModsConfig.BiotechActive && pawn.genes != null && styleItem.requiredGene != null && !pawn.genes.HasActiveGene(styleItem.requiredGene))
		{
			return 0f;
		}
		StyleGender gender = pawn.Ideo.style.GetGender(styleItem);
		if (pawn.gender == Gender.Male)
		{
			switch (gender)
			{
			case StyleGender.Female:
				return 1f;
			case StyleGender.FemaleUsually:
				return 5f;
			case StyleGender.MaleUsually:
				return 30f;
			case StyleGender.Male:
				return 70f;
			case StyleGender.Any:
				return 60f;
			}
		}
		if (pawn.gender == Gender.Female)
		{
			switch (gender)
			{
			case StyleGender.Female:
				return 70f;
			case StyleGender.FemaleUsually:
				return 30f;
			case StyleGender.MaleUsually:
				return 5f;
			case StyleGender.Male:
				return 1f;
			case StyleGender.Any:
				return 60f;
			}
		}
		Log.Error("Unknown hair likelihood for " + styleItem?.ToString() + " with " + pawn);
		return 0f;
	}
}
