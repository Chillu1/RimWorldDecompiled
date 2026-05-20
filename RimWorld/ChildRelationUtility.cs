using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ChildRelationUtility
{
	public const float MinFemaleAgeToHaveChildren = 16f;

	public const float MaxFemaleAgeToHaveChildren = 45f;

	public const float UsualFemaleAgeToHaveChildren = 27f;

	public const float MinMaleAgeToHaveChildren = 14f;

	public const float MaxMaleAgeToHaveChildren = 50f;

	public const float UsualMaleAgeToHaveChildren = 30f;

	public const float ChanceForChildToHaveNameOfAnyParent = 0.99f;

	public static float ChanceOfBecomingChildOf(Pawn child, Pawn father, Pawn mother, PawnGenerationRequest? childGenerationRequest, PawnGenerationRequest? fatherGenerationRequest, PawnGenerationRequest? motherGenerationRequest)
	{
		if (child.IsDuplicate)
		{
			return 0f;
		}
		if (father != null && father.gender != Gender.Male)
		{
			Log.Warning("Tried to calculate chance for father with gender \"" + father.gender.ToString() + "\".");
			return 0f;
		}
		if (mother != null && mother.gender != Gender.Female)
		{
			Log.Warning("Tried to calculate chance for mother with gender \"" + mother.gender.ToString() + "\".");
			return 0f;
		}
		if (father != null && child.GetFather() != null && child.GetFather() != father)
		{
			return 0f;
		}
		if (mother != null && child.GetMother() != null && child.GetMother() != mother)
		{
			return 0f;
		}
		if (mother != null && father != null && !LovePartnerRelationUtility.LovePartnerRelationExists(mother, father) && !LovePartnerRelationUtility.ExLovePartnerRelationExists(mother, father))
		{
			return 0f;
		}
		if (mother != null && !XenotypesCompatible(child, mother))
		{
			return 0f;
		}
		if (father != null && !XenotypesCompatible(child, father))
		{
			return 0f;
		}
		if (ModsConfig.BiotechActive)
		{
			if (father?.records != null && father.records.GetValue(RecordDefOf.TimeAsChildInColony) > 0f)
			{
				return 0f;
			}
			if (mother?.records != null && mother.records.GetValue(RecordDefOf.TimeAsChildInColony) > 0f)
			{
				return 0f;
			}
		}
		float num = 1f;
		float num2 = 1f;
		float num3 = 1f;
		float num4 = 1f;
		if (father != null && child.GetFather() == null)
		{
			num = GetParentAgeFactor(father, child, 14f, 30f, 50f);
			if (num == 0f)
			{
				return 0f;
			}
			if (father.story.traits.HasTrait(TraitDefOf.Gay))
			{
				num4 = 0.1f;
			}
		}
		if (mother != null && child.GetMother() == null)
		{
			num2 = GetParentAgeFactor(mother, child, 16f, 27f, 45f);
			if (num2 == 0f)
			{
				return 0f;
			}
			int num5 = NumberOfChildrenFemaleWantsEver(mother);
			if (mother.relations.ChildrenCount >= num5)
			{
				return 0f;
			}
			num3 = 1f - (float)mother.relations.ChildrenCount / (float)num5;
			if (mother.story.traits.HasTrait(TraitDefOf.Gay))
			{
				num4 = 0.1f;
			}
		}
		float num6 = 1f;
		if (mother != null)
		{
			Pawn firstDirectRelationPawn = mother.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
			if (firstDirectRelationPawn != null && firstDirectRelationPawn != father)
			{
				num6 *= 0.15f;
			}
		}
		if (father != null)
		{
			Pawn firstDirectRelationPawn2 = father.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
			if (firstDirectRelationPawn2 != null && firstDirectRelationPawn2 != mother)
			{
				num6 *= 0.15f;
			}
		}
		return num * num2 * num3 * num6 * num4;
	}

	private static float GetParentAgeFactor(Pawn parent, Pawn child, float minAgeToHaveChildren, float usualAgeToHaveChildren, float maxAgeToHaveChildren)
	{
		float num = PawnRelationUtility.MaxPossibleBioAgeAt(parent.ageTracker.AgeBiologicalYearsFloat, parent.ageTracker.AgeChronologicalYearsFloat, child.ageTracker.AgeChronologicalYearsFloat);
		float num2 = PawnRelationUtility.MinPossibleBioAgeAt(parent.ageTracker.AgeBiologicalYearsFloat, child.ageTracker.AgeChronologicalYearsFloat);
		if (num <= 0f)
		{
			return 0f;
		}
		if (num2 > num)
		{
			return 0f;
		}
		if (num2 <= usualAgeToHaveChildren && num >= usualAgeToHaveChildren)
		{
			return 1f;
		}
		float ageFactor = GetAgeFactor(num2, minAgeToHaveChildren, maxAgeToHaveChildren, usualAgeToHaveChildren);
		float ageFactor2 = GetAgeFactor(num, minAgeToHaveChildren, maxAgeToHaveChildren, usualAgeToHaveChildren);
		return Mathf.Max(ageFactor, ageFactor2);
	}

	public static bool ChildWantsNameOfAnyParent(Pawn child)
	{
		return Rand.ValueSeeded(child.thingIDNumber ^ 0x542EAFC) < 0.99f;
	}

	private static int NumberOfChildrenFemaleWantsEver(Pawn female)
	{
		Rand.PushState();
		Rand.Seed = female.thingIDNumber * 3;
		int result = Rand.RangeInclusive(0, 3);
		Rand.PopState();
		return result;
	}

	private static float GetAgeFactor(float ageAtBirth, float min, float max, float mid)
	{
		return GenMath.GetFactorInInterval(min, mid, max, 1.6f, ageAtBirth);
	}

	public static bool DefinitelyHasNotBirthName(Pawn pawn)
	{
		if (!(pawn.Name is NameTriple nameTriple))
		{
			return true;
		}
		List<Pawn> spouses = pawn.GetSpouses(includeDead: true);
		if (!spouses.Any())
		{
			return false;
		}
		for (int i = 0; i < spouses.Count; i++)
		{
			Pawn pawn2 = spouses[i];
			if (pawn2.Name is NameTriple { Last: var last } && !(nameTriple.Last != last) && ((pawn2.GetMother() != null && pawn2.GetMother().Name is NameTriple nameTriple3 && nameTriple3.Last == last) || (pawn2.GetFather() != null && pawn2.GetFather().Name is NameTriple nameTriple4 && nameTriple4.Last == last)))
			{
				return true;
			}
		}
		return false;
	}

	public static bool XenotypesCompatible(Pawn first, Pawn second)
	{
		if (!ModsConfig.BiotechActive)
		{
			return true;
		}
		if (first.genes == null || second.genes == null)
		{
			return false;
		}
		if ((first.genes.UniqueXenotype || second.genes.UniqueXenotype) && !GeneUtility.SameHeritableXenotype(first, second))
		{
			return false;
		}
		if (first.genes.Xenotype.inheritable && first.genes.Xenotype != second.genes.Xenotype)
		{
			return false;
		}
		if (second.genes.Xenotype.inheritable && second.genes.Xenotype != first.genes.Xenotype)
		{
			return false;
		}
		return true;
	}
}
