using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Sibling : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			if (me.GetMother() != null && me.GetFather() != null && me.GetMother() == other.GetMother() && me.GetFather() == other.GetFather())
			{
				return true;
			}
			return false;
		}

		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			float num = 1f;
			float num2 = 1f;
			if (other.GetFather() == null && other.GetMother() == null)
			{
				num2 = ((!request.FixedMelanin.HasValue) ? PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin) : ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin));
			}
			else
			{
				num = ChildRelationUtility.ChanceOfBecomingChildOf(generated, other.GetFather(), other.GetMother(), request, null, null);
			}
			float num3 = Mathf.Abs(generated.ageTracker.AgeChronologicalYearsFloat - other.ageTracker.AgeChronologicalYearsFloat);
			float num4 = 1f;
			if (num3 > 40f)
			{
				num4 = 0.2f;
			}
			else if (num3 > 10f)
			{
				num4 = 0.65f;
			}
			return num * num2 * num4 * BaseGenerationChanceFactor(generated, other, request);
		}

		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			bool num = other.GetMother() != null;
			bool flag = other.GetFather() != null;
			bool flag2 = Rand.Value < 0.85f;
			if (num && LovePartnerRelationUtility.HasAnyLovePartner(other.GetMother()))
			{
				flag2 = false;
			}
			if (flag && LovePartnerRelationUtility.HasAnyLovePartner(other.GetFather()))
			{
				flag2 = false;
			}
			if (!num)
			{
				Pawn newMother = GenerateParent(generated, other, Gender.Female, request, flag2);
				other.SetMother(newMother);
			}
			generated.SetMother(other.GetMother());
			if (!flag)
			{
				Pawn newFather = GenerateParent(generated, other, Gender.Male, request, flag2);
				other.SetFather(newFather);
			}
			generated.SetFather(other.GetFather());
			if (!num || !flag)
			{
				if (other.GetMother().story.traits.HasTrait(TraitDefOf.Gay) || other.GetFather().story.traits.HasTrait(TraitDefOf.Gay))
				{
					other.GetFather().relations.AddDirectRelation(PawnRelationDefOf.ExLover, other.GetMother());
				}
				else if (flag2)
				{
					Pawn mother = other.GetMother();
					Pawn father = other.GetFather();
					NameTriple nameTriple = mother.Name as NameTriple;
					father.relations.AddDirectRelation(PawnRelationDefOf.Spouse, mother);
					if (nameTriple != null)
					{
						PawnGenerationRequest request2 = default(PawnGenerationRequest);
						SpouseRelationUtility.ResolveNameForSpouseOnGeneration(ref request2, mother);
						string b = nameTriple.Last;
						string text = null;
						if (request2.FixedLastName != null)
						{
							b = request2.FixedLastName;
						}
						if (request2.FixedBirthName != null)
						{
							text = request2.FixedBirthName;
						}
						if (mother.story != null && (nameTriple.Last != b || mother.story.birthLastName != text))
						{
							mother.story.birthLastName = text;
						}
					}
				}
				else
				{
					LovePartnerRelationUtility.GiveRandomExLoverOrExSpouseRelation(other.GetFather(), other.GetMother());
				}
			}
			ResolveMyName(ref request, generated);
			ResolveMySkinColor(ref request, generated);
		}

		private static Pawn GenerateParent(Pawn generatedChild, Pawn existingChild, Gender genderToGenerate, PawnGenerationRequest childRequest, bool newlyGeneratedParentsWillBeSpousesIfNotGay)
		{
			float ageChronologicalYearsFloat = generatedChild.ageTracker.AgeChronologicalYearsFloat;
			float ageChronologicalYearsFloat2 = existingChild.ageTracker.AgeChronologicalYearsFloat;
			float num = ((genderToGenerate == Gender.Male) ? 14f : 16f);
			float num2 = ((genderToGenerate == Gender.Male) ? 50f : 45f);
			float num3 = ((genderToGenerate == Gender.Male) ? 30f : 27f);
			float num4 = Mathf.Max(ageChronologicalYearsFloat, ageChronologicalYearsFloat2) + num;
			float maxChronologicalAge = num4 + (num2 - num);
			float midChronologicalAge = num4 + (num3 - num);
			GenerateParentParams(num4, maxChronologicalAge, midChronologicalAge, num, generatedChild, existingChild, childRequest, out var biologicalAge, out var chronologicalAge, out var melanin, out var lastName);
			bool allowGay = true;
			Faction faction = existingChild.Faction;
			if (faction == null || faction.IsPlayer)
			{
				bool tryMedievalOrBetter = faction != null && (int)faction.def.techLevel >= 3;
				if (!Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction_NewTemp(out faction, tryMedievalOrBetter, allowDefeated: true))
				{
					faction = Faction.OfAncients;
				}
			}
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(existingChild.kindDef, faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, newborn: false, allowDead: true, allowDowned: true, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, null, 1f, null, null, null, null, null, biologicalAge, chronologicalAge, genderToGenerate, melanin, lastName));
			if (!Find.WorldPawns.Contains(pawn))
			{
				Find.WorldPawns.PassToWorld(pawn);
			}
			return pawn;
		}

		private static void GenerateParentParams(float minChronologicalAge, float maxChronologicalAge, float midChronologicalAge, float minBioAgeToHaveChildren, Pawn generatedChild, Pawn existingChild, PawnGenerationRequest childRequest, out float biologicalAge, out float chronologicalAge, out float melanin, out string lastName)
		{
			chronologicalAge = Rand.GaussianAsymmetric(midChronologicalAge, (midChronologicalAge - minChronologicalAge) / 2f, (maxChronologicalAge - midChronologicalAge) / 2f);
			chronologicalAge = Mathf.Clamp(chronologicalAge, minChronologicalAge, maxChronologicalAge);
			biologicalAge = Rand.Range(minBioAgeToHaveChildren, Mathf.Min(existingChild.RaceProps.lifeExpectancy, chronologicalAge));
			if (existingChild.GetFather() != null)
			{
				melanin = ParentRelationUtility.GetRandomSecondParentSkinColor(existingChild.GetFather().story.melanin, existingChild.story.melanin, childRequest.FixedMelanin);
			}
			else if (existingChild.GetMother() != null)
			{
				melanin = ParentRelationUtility.GetRandomSecondParentSkinColor(existingChild.GetMother().story.melanin, existingChild.story.melanin, childRequest.FixedMelanin);
			}
			else if (!childRequest.FixedMelanin.HasValue)
			{
				melanin = PawnSkinColors.GetRandomMelaninSimilarTo(existingChild.story.melanin);
			}
			else
			{
				float num = Mathf.Min(childRequest.FixedMelanin.Value, existingChild.story.melanin);
				float num2 = Mathf.Max(childRequest.FixedMelanin.Value, existingChild.story.melanin);
				if (Rand.Value < 0.5f)
				{
					melanin = PawnSkinColors.GetRandomMelaninSimilarTo(num, 0f, num);
				}
				else
				{
					melanin = PawnSkinColors.GetRandomMelaninSimilarTo(num2, num2);
				}
			}
			lastName = null;
			if (ChildRelationUtility.DefinitelyHasNotBirthName(existingChild) || !ChildRelationUtility.ChildWantsNameOfAnyParent(existingChild))
			{
				return;
			}
			if (existingChild.GetMother() == null && existingChild.GetFather() == null)
			{
				if (Rand.Value < 0.5f)
				{
					lastName = ((NameTriple)existingChild.Name).Last;
				}
				return;
			}
			string last = ((NameTriple)existingChild.Name).Last;
			string b = null;
			if (existingChild.GetMother() != null)
			{
				b = ((NameTriple)existingChild.GetMother().Name).Last;
			}
			else if (existingChild.GetFather() != null)
			{
				b = ((NameTriple)existingChild.GetFather().Name).Last;
			}
			if (last != b)
			{
				lastName = last;
			}
		}

		private static void ResolveMyName(ref PawnGenerationRequest request, Pawn generated)
		{
			if (request.FixedLastName == null && ChildRelationUtility.ChildWantsNameOfAnyParent(generated))
			{
				if (Rand.Value < 0.5f)
				{
					request.SetFixedLastName(((NameTriple)generated.GetFather().Name).Last);
				}
				else
				{
					request.SetFixedLastName(((NameTriple)generated.GetMother().Name).Last);
				}
			}
		}

		private static void ResolveMySkinColor(ref PawnGenerationRequest request, Pawn generated)
		{
			if (!request.FixedMelanin.HasValue)
			{
				request.SetFixedMelanin(ChildRelationUtility.GetRandomChildSkinColor(generated.GetFather().story.melanin, generated.GetMother().story.melanin));
			}
		}
	}
}
