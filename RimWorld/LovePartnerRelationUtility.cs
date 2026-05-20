using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class LovePartnerRelationUtility
{
	private const float MinAgeToGenerateWithLovePartnerRelation = 14f;

	private static List<DirectPawnRelation> tmpExistingLovePartners = new List<DirectPawnRelation>();

	public static bool HasAnyLovePartner(Pawn pawn, bool allowDead = true)
	{
		return ExistingLovePartner(pawn, allowDead) != null;
	}

	public static bool IsLovePartnerRelation(PawnRelationDef relation)
	{
		if (relation != PawnRelationDefOf.Lover && relation != PawnRelationDefOf.Fiance)
		{
			return relation == PawnRelationDefOf.Spouse;
		}
		return true;
	}

	public static bool IsExLovePartnerRelation(PawnRelationDef relation)
	{
		if (relation != PawnRelationDefOf.ExLover)
		{
			return relation == PawnRelationDefOf.ExSpouse;
		}
		return true;
	}

	public static bool HasAnyLovePartnerOfTheSameGender(Pawn pawn)
	{
		return pawn.relations.DirectRelations.Find((DirectPawnRelation x) => IsLovePartnerRelation(x.def) && x.otherPawn.gender == pawn.gender) != null;
	}

	public static bool HasAnyExLovePartnerOfTheSameGender(Pawn pawn)
	{
		return pawn.relations.DirectRelations.Find((DirectPawnRelation x) => IsExLovePartnerRelation(x.def) && x.otherPawn.gender == pawn.gender) != null;
	}

	public static bool HasAnyLovePartnerOfTheOppositeGender(Pawn pawn)
	{
		return pawn.relations.DirectRelations.Find((DirectPawnRelation x) => IsLovePartnerRelation(x.def) && x.otherPawn.gender != pawn.gender) != null;
	}

	public static bool HasAnyExLovePartnerOfTheOppositeGender(Pawn pawn)
	{
		return pawn.relations.DirectRelations.Find((DirectPawnRelation x) => IsExLovePartnerRelation(x.def) && x.otherPawn.gender != pawn.gender) != null;
	}

	public static Pawn ExistingLovePartner(Pawn pawn, bool allowDead = true)
	{
		List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if (IsLovePartnerRelation(directRelations[i].def) && (!directRelations[i].otherPawn.Destroyed || allowDead))
			{
				return directRelations[i].otherPawn;
			}
		}
		return null;
	}

	public static List<DirectPawnRelation> ExistingLovePartners(Pawn pawn, bool allowDead = true)
	{
		tmpExistingLovePartners.Clear();
		List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if (IsLovePartnerRelation(directRelations[i].def) && (!directRelations[i].otherPawn.Destroyed || allowDead))
			{
				tmpExistingLovePartners.Add(directRelations[i]);
			}
		}
		return tmpExistingLovePartners;
	}

	public static DirectPawnRelation ExistingLoveRealtionshipBetween(Pawn pawn, Pawn other, bool allowDead = true)
	{
		return (from r in ExistingLovePartners(pawn, allowDead)
			where r.otherPawn == other
			select r).FirstOrDefault();
	}

	public static int ExistingLovePartnersCount(Pawn pawn, bool allowDead = true)
	{
		int num = 0;
		List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if (IsLovePartnerRelation(directRelations[i].def) && (!directRelations[i].otherPawn.Destroyed || allowDead))
			{
				num++;
			}
		}
		return num;
	}

	public static bool LovePartnerRelationExists(Pawn first, Pawn second)
	{
		if (!first.relations.DirectRelationExists(PawnRelationDefOf.Lover, second) && !first.relations.DirectRelationExists(PawnRelationDefOf.Fiance, second))
		{
			return first.relations.DirectRelationExists(PawnRelationDefOf.Spouse, second);
		}
		return true;
	}

	public static bool ExLovePartnerRelationExists(Pawn first, Pawn second)
	{
		if (!first.relations.DirectRelationExists(PawnRelationDefOf.ExSpouse, second))
		{
			return first.relations.DirectRelationExists(PawnRelationDefOf.ExLover, second);
		}
		return true;
	}

	public static void GiveRandomExLoverOrExSpouseRelation(Pawn first, Pawn second)
	{
		PawnRelationDef def = ((!(Rand.Value < 0.5f)) ? PawnRelationDefOf.ExSpouse : PawnRelationDefOf.ExLover);
		first.relations.AddDirectRelation(def, second);
	}

	public static Pawn GetPartnerInMyBed(Pawn pawn)
	{
		Building_Bed building_Bed = pawn.CurrentBed();
		if (building_Bed == null)
		{
			return null;
		}
		if (building_Bed.SleepingSlotsCount <= 1)
		{
			return null;
		}
		if (!HasAnyLovePartner(pawn))
		{
			return null;
		}
		foreach (Pawn curOccupant in building_Bed.CurOccupants)
		{
			if (curOccupant != pawn && LovePartnerRelationExists(pawn, curOccupant))
			{
				return curOccupant;
			}
		}
		return null;
	}

	public static Pawn ExistingLeastLikedPawnWithRelation(Pawn p, Func<DirectPawnRelation, bool> validator)
	{
		return ExistingLeastLikedRel(p, validator)?.otherPawn;
	}

	public static DirectPawnRelation ExistingLeastLikedRel(Pawn p, Func<DirectPawnRelation, bool> validator)
	{
		if (!p.RaceProps.IsFlesh)
		{
			return null;
		}
		DirectPawnRelation directPawnRelation = null;
		int num = int.MaxValue;
		List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if (validator(directRelations[i]))
			{
				int num2 = p.relations.OpinionOf(directRelations[i].otherPawn);
				if (directPawnRelation == null || num2 < num)
				{
					directPawnRelation = directRelations[i];
					num = num2;
				}
			}
		}
		return directPawnRelation;
	}

	public static Pawn ExistingMostLikedLovePartner(Pawn p, bool allowDead)
	{
		return ExistingMostLikedLovePartnerRel(p, allowDead)?.otherPawn;
	}

	public static DirectPawnRelation ExistingMostLikedLovePartnerRel(Pawn p, bool allowDead)
	{
		if (!p.RaceProps.IsFlesh)
		{
			return null;
		}
		DirectPawnRelation directPawnRelation = null;
		int num = int.MinValue;
		List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if ((allowDead || !directRelations[i].otherPawn.Dead) && IsLovePartnerRelation(directRelations[i].def))
			{
				int num2 = p.relations.OpinionOf(directRelations[i].otherPawn);
				if (directPawnRelation == null || num2 > num)
				{
					directPawnRelation = directRelations[i];
					num = num2;
				}
			}
		}
		return directPawnRelation;
	}

	public static HistoryEventDef GetHistoryEventLoveRelationCount(this Pawn pawn)
	{
		int count = pawn.GetLoveRelations(includeDead: false).Count;
		if (count <= 1)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_OneOrFewer;
		}
		if (count <= 2)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Two;
		}
		if (count <= 3)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Three;
		}
		if (count <= 4)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Four;
		}
		return HistoryEventDefOf.GotMarried_SpouseCount_FiveOrMore;
	}

	public static HistoryEventDef GetHistoryEventForLoveRelationCountPlusOne(this Pawn pawn)
	{
		int count = pawn.GetLoveRelations(includeDead: false).Count;
		if (count == 0)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_OneOrFewer;
		}
		if (count < 2)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Two;
		}
		if (count < 3)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Three;
		}
		if (count < 4)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Four;
		}
		return HistoryEventDefOf.GotMarried_SpouseCount_FiveOrMore;
	}

	public static float GetLovinMtbHours(Pawn pawn, Pawn partner)
	{
		if (pawn.Dead || partner.Dead)
		{
			return -1f;
		}
		if (DebugSettings.alwaysDoLovin)
		{
			return 0.1f;
		}
		if (pawn.needs?.food?.Starving == true || partner.needs?.food?.Starving == true)
		{
			return -1f;
		}
		if (pawn.health.hediffSet.BleedRateTotal > 0f || partner.health.hediffSet.BleedRateTotal > 0f)
		{
			return -1f;
		}
		if (pawn.health.hediffSet.InLabor() || partner.health.hediffSet.InLabor())
		{
			return -1f;
		}
		float num = LovinMtbSinglePawnFactor(pawn);
		if (num <= 0f)
		{
			return -1f;
		}
		float num2 = LovinMtbSinglePawnFactor(partner);
		if (num2 <= 0f)
		{
			return -1f;
		}
		float num3 = 12f;
		num3 *= num;
		num3 *= num2;
		num3 /= Mathf.Max(pawn.relations.SecondaryLovinChanceFactor(partner), 0.1f);
		num3 /= Mathf.Max(partner.relations.SecondaryLovinChanceFactor(pawn), 0.1f);
		num3 *= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, pawn.relations.OpinionOf(partner));
		num3 *= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, partner.relations.OpinionOf(pawn));
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicLove))
		{
			num3 /= 4f;
		}
		return num3;
	}

	private static float LovinMtbSinglePawnFactor(Pawn pawn)
	{
		float num = 1f;
		num /= 1f - pawn.health.hediffSet.PainTotal;
		float level = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
		if (level < 0.5f)
		{
			num /= level * 2f;
		}
		return num / GenMath.FlatHill(0f, 14f, 16f, 25f, 80f, 0.2f, pawn.ageTracker.AgeBiologicalYearsFloat);
	}

	public static void TryToShareBed(Pawn first, Pawn second)
	{
		if (!TryToShareBed_Int(first, second))
		{
			TryToShareBed_Int(second, first);
		}
	}

	private static bool TryToShareBed_Int(Pawn bedOwner, Pawn otherPawn)
	{
		Building_Bed ownedBed = bedOwner.ownership.OwnedBed;
		if (ownedBed != null && ownedBed.AnyUnownedSleepingSlot && BedUtility.WillingToShareBed(bedOwner, otherPawn))
		{
			otherPawn.ownership.ClaimBedIfNonMedical(ownedBed);
			return true;
		}
		return false;
	}

	public static float LovePartnerRelationGenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
	{
		if (generated.ageTracker.AgeBiologicalYearsFloat < 14f)
		{
			return 0f;
		}
		if (other.ageTracker.AgeBiologicalYearsFloat < 14f)
		{
			return 0f;
		}
		if (other.story == null)
		{
			return 0f;
		}
		if (generated.gender == other.gender && (!other.story.traits.HasTrait(TraitDefOf.Gay) || !request.AllowGay))
		{
			return 0f;
		}
		if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay))
		{
			return 0f;
		}
		if (ModsConfig.BiotechActive)
		{
			if (generated?.records != null && generated.records.GetValue(RecordDefOf.TimeAsChildInColony) > 0f)
			{
				return 0f;
			}
			if (other?.records != null && other.records.GetValue(RecordDefOf.TimeAsChildInColony) > 0f)
			{
				return 0f;
			}
		}
		float num = 1f;
		if (ex)
		{
			int num2 = 0;
			List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (IsExLovePartnerRelation(directRelations[i].def))
				{
					num2++;
				}
			}
			num = Mathf.Pow(0.2f, num2);
		}
		else if (HasAnyLovePartner(other))
		{
			return 0f;
		}
		float num3 = ((generated.gender == other.gender) ? 0.01f : 1f);
		float generationChanceAgeFactor = GetGenerationChanceAgeFactor(generated);
		float generationChanceAgeFactor2 = GetGenerationChanceAgeFactor(other);
		float generationChanceAgeGapFactor = GetGenerationChanceAgeGapFactor(generated, other, ex);
		float num4 = 1f;
		if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
		{
			num4 = 0.01f;
		}
		return num * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * num3 * num4;
	}

	private static float GetGenerationChanceAgeFactor(Pawn p)
	{
		return Mathf.Clamp(GenMath.LerpDouble(14f, 27f, 0f, 1f, p.ageTracker.AgeBiologicalYearsFloat), 0f, 1f);
	}

	private static float GetGenerationChanceAgeGapFactor(Pawn p1, Pawn p2, bool ex)
	{
		float num = Mathf.Abs(p1.ageTracker.AgeBiologicalYearsFloat - p2.ageTracker.AgeBiologicalYearsFloat);
		if (ex)
		{
			float num2 = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p1, p2);
			if (num2 >= 0f)
			{
				num = Mathf.Min(num, num2);
			}
			float num3 = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p2, p1);
			if (num3 >= 0f)
			{
				num = Mathf.Min(num, num3);
			}
		}
		if (num > 40f)
		{
			return 0f;
		}
		return Mathf.Clamp(GenMath.LerpDouble(0f, 20f, 1f, 0.001f, num), 0.001f, 1f);
	}

	private static float MinPossibleAgeGapAtMinAgeToGenerateAsLovers(Pawn p1, Pawn p2)
	{
		float num = p1.ageTracker.AgeChronologicalYearsFloat - 14f;
		if (num < 0f)
		{
			return 0f;
		}
		float num2 = PawnRelationUtility.MaxPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, p2.ageTracker.AgeChronologicalYearsFloat, num);
		float num3 = PawnRelationUtility.MinPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, num);
		if (num2 < 0f)
		{
			return -1f;
		}
		if (num2 < 14f)
		{
			return -1f;
		}
		if (num3 <= 14f)
		{
			return 0f;
		}
		return num3 - 14f;
	}

	public static void TryToShareChildrenForGeneratedLovePartner(Pawn generated, Pawn other, PawnGenerationRequest request, float extraChanceFactor)
	{
		if (generated.gender == other.gender)
		{
			return;
		}
		List<Pawn> list = other.relations.Children.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Pawn pawn = list[i];
			float num = 1f;
			if (generated.gender == Gender.Male)
			{
				num = ChildRelationUtility.ChanceOfBecomingChildOf(pawn, generated, other, null, request, null);
			}
			else if (generated.gender == Gender.Female)
			{
				num = ChildRelationUtility.ChanceOfBecomingChildOf(pawn, other, generated, null, null, request);
			}
			num *= extraChanceFactor;
			if (Rand.Value < num)
			{
				if (generated.gender == Gender.Male)
				{
					pawn.SetFather(generated);
				}
				else if (generated.gender == Gender.Female)
				{
					pawn.SetMother(generated);
				}
			}
		}
	}

	public static void ChangeSpouseRelationsToExSpouse(Pawn pawn)
	{
		List<Pawn> spouses = pawn.GetSpouses(includeDead: true);
		for (int num = spouses.Count - 1; num >= 0; num--)
		{
			HistoryEvent ev = new HistoryEvent(pawn.GetHistoryEventForSpouseCountPlusOne(), pawn.Named(HistoryEventArgsNames.Doer));
			if (spouses[num].Dead || !ev.DoerWillingToDo())
			{
				pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, spouses[num]);
				pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, spouses[num]);
			}
		}
	}

	public static Pawn GetMostDislikedNonPartnerBedOwner(Pawn p)
	{
		Building_Bed ownedBed = p.ownership.OwnedBed;
		if (ownedBed == null)
		{
			return null;
		}
		Pawn pawn = null;
		int num = 0;
		for (int i = 0; i < ownedBed.OwnersForReading.Count; i++)
		{
			if (ownedBed.OwnersForReading[i] != p && !LovePartnerRelationExists(p, ownedBed.OwnersForReading[i]))
			{
				int num2 = p.relations.OpinionOf(ownedBed.OwnersForReading[i]);
				if (pawn == null || num2 < num)
				{
					pawn = ownedBed.OwnersForReading[i];
					num = num2;
				}
			}
		}
		return pawn;
	}

	public static float IncestOpinionOffsetFor(Pawn other, Pawn pawn)
	{
		float num = 0f;
		List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if (!IsLovePartnerRelation(directRelations[i].def) || directRelations[i].otherPawn == pawn || directRelations[i].otherPawn.Dead)
			{
				continue;
			}
			foreach (PawnRelationDef relation in other.GetRelations(directRelations[i].otherPawn))
			{
				float incestOpinionOffset = relation.incestOpinionOffset;
				if (incestOpinionOffset < num)
				{
					num = incestOpinionOffset;
				}
			}
		}
		return num;
	}

	public static bool AreNearEachOther(Pawn p1, Pawn p2)
	{
		if (p1.DestroyedOrNull() || p2.DestroyedOrNull())
		{
			return false;
		}
		if (p1.MapHeld != null && p1.MapHeld == p2.MapHeld)
		{
			return true;
		}
		if (p1.GetCaravan() != null && p1.GetCaravan() == p2.GetCaravan())
		{
			return true;
		}
		if (p1.ParentHolder != null && p1.ParentHolder == p2.ParentHolder)
		{
			return true;
		}
		return false;
	}
}
