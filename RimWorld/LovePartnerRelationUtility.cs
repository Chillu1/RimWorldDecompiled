using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class LovePartnerRelationUtility
	{
		private const float MinAgeToGenerateWithLovePartnerRelation = 14f;

		public static bool HasAnyLovePartner(Pawn pawn)
		{
			if (pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse) == null && pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover) == null)
			{
				return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance) != null;
			}
			return true;
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

		public static Pawn ExistingLovePartner(Pawn pawn)
		{
			Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
			if (firstDirectRelationPawn != null)
			{
				return firstDirectRelationPawn;
			}
			firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
			if (firstDirectRelationPawn != null)
			{
				return firstDirectRelationPawn;
			}
			firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
			if (firstDirectRelationPawn != null)
			{
				return firstDirectRelationPawn;
			}
			return null;
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
			PawnRelationDef def = (!(Rand.Value < 0.5f)) ? PawnRelationDefOf.ExSpouse : PawnRelationDefOf.ExLover;
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
			if (pawn.needs.food.Starving || partner.needs.food.Starving)
			{
				return -1f;
			}
			if (pawn.health.hediffSet.BleedRateTotal > 0f || partner.health.hediffSet.BleedRateTotal > 0f)
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
			return 12f * num * num2 / Mathf.Max(pawn.relations.SecondaryLovinChanceFactor(partner), 0.1f) / Mathf.Max(partner.relations.SecondaryLovinChanceFactor(pawn), 0.1f) * GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, pawn.relations.OpinionOf(partner)) * GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, partner.relations.OpinionOf(pawn));
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
			if (ownedBed != null && ownedBed.AnyUnownedSleepingSlot)
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
			if (generated.gender == other.gender && (!other.story.traits.HasTrait(TraitDefOf.Gay) || !request.AllowGay))
			{
				return 0f;
			}
			if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay))
			{
				return 0f;
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
			float num3 = (generated.gender == other.gender) ? 0.01f : 1f;
			float generationChanceAgeFactor = GetGenerationChanceAgeFactor(generated);
			float generationChanceAgeFactor2 = GetGenerationChanceAgeFactor(other);
			float generationChanceAgeGapFactor = GetGenerationChanceAgeGapFactor(generated, other, ex);
			float num4 = 1f;
			if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
			{
				num4 = 0.01f;
			}
			float num5 = 1f;
			num5 = ((!request.FixedMelanin.HasValue) ? PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin) : ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin));
			return num * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * num3 * num5 * num4;
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
				Log.Warning("at < 0");
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
			while (true)
			{
				Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
				if (firstDirectRelationPawn != null)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, firstDirectRelationPawn);
					pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, firstDirectRelationPawn);
					continue;
				}
				break;
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
				if (IsLovePartnerRelation(directRelations[i].def) && directRelations[i].otherPawn != pawn && !directRelations[i].otherPawn.Dead)
				{
					foreach (PawnRelationDef relation in other.GetRelations(directRelations[i].otherPawn))
					{
						float incestOpinionOffset = relation.incestOpinionOffset;
						if (incestOpinionOffset < num)
						{
							num = incestOpinionOffset;
						}
					}
				}
			}
			return num;
		}
	}
}
