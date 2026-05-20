using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnRelationWorker_Sibling : PawnRelationWorker
{
	public override bool InRelation(Pawn me, Pawn other)
	{
		if (me == other)
		{
			return false;
		}
		if (me.HasSameMother(other))
		{
			return me.HasSameFather(other);
		}
		return false;
	}

	public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
	{
		float num = 1f;
		if (other.GetFather() != null || other.GetMother() != null)
		{
			num = ChildRelationUtility.ChanceOfBecomingChildOf(generated, other.GetFather(), other.GetMother(), request, null, null);
		}
		if (!ChildRelationUtility.XenotypesCompatible(generated, other))
		{
			return 0f;
		}
		float num2 = Mathf.Abs(generated.ageTracker.AgeChronologicalYearsFloat - other.ageTracker.AgeChronologicalYearsFloat);
		float num3 = 1f;
		if (num2 > 40f)
		{
			num3 = 0.2f;
		}
		else if (num2 > 10f)
		{
			num3 = 0.65f;
		}
		return num * num3 * BaseGenerationChanceFactor(generated, other, request);
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
					string text = nameTriple.Last;
					string text2 = null;
					if (request2.FixedLastName != null)
					{
						text = request2.FixedLastName;
					}
					if (request2.FixedBirthName != null)
					{
						text2 = request2.FixedBirthName;
					}
					if (mother.story != null && (nameTriple.Last != text || mother.story.birthLastName != text2))
					{
						mother.story.birthLastName = text2;
					}
				}
			}
			else
			{
				LovePartnerRelationUtility.GiveRandomExLoverOrExSpouseRelation(other.GetFather(), other.GetMother());
			}
		}
		ResolveMyName(ref request, generated);
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
		GenerateParentParams(num4, maxChronologicalAge, midChronologicalAge, num, generatedChild, existingChild, childRequest, out var biologicalAge, out var chronologicalAge, out var lastName, out var xenotype);
		bool flag = true;
		Faction faction = existingChild.Faction;
		if (faction == null || faction.IsPlayer || !faction.def.humanlikeFaction)
		{
			bool tryMedievalOrBetter = faction != null && (int)faction.def.techLevel >= 3;
			if (!Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter, allowDefeated: true))
			{
				faction = Faction.OfAncients;
			}
		}
		PawnKindDef kindDef = existingChild.kindDef;
		Faction faction2 = faction;
		bool allowGay = flag;
		float? fixedBiologicalAge = biologicalAge;
		float? fixedChronologicalAge = chronologicalAge;
		Gender? fixedGender = genderToGenerate;
		string fixedLastName = lastName;
		XenotypeDef forcedXenotype = xenotype;
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef, faction2, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: true, allowDowned: true, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge, fixedChronologicalAge, fixedGender, fixedLastName, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, forcedXenotype));
		if (!Find.WorldPawns.Contains(pawn))
		{
			Find.WorldPawns.PassToWorld(pawn);
		}
		return pawn;
	}

	private static void GenerateParentParams(float minChronologicalAge, float maxChronologicalAge, float midChronologicalAge, float minBioAgeToHaveChildren, Pawn generatedChild, Pawn existingChild, PawnGenerationRequest childRequest, out float biologicalAge, out float chronologicalAge, out string lastName, out XenotypeDef xenotype)
	{
		chronologicalAge = Rand.GaussianAsymmetric(midChronologicalAge, (midChronologicalAge - minChronologicalAge) / 2f, (maxChronologicalAge - midChronologicalAge) / 2f);
		chronologicalAge = Mathf.Clamp(chronologicalAge, minChronologicalAge, maxChronologicalAge);
		biologicalAge = Rand.Range(minBioAgeToHaveChildren, Mathf.Min(existingChild.RaceProps.lifeExpectancy, chronologicalAge));
		lastName = null;
		xenotype = null;
		if (ModsConfig.BiotechActive)
		{
			if (existingChild.genes.Xenotype.inheritable)
			{
				xenotype = existingChild.genes.Xenotype;
			}
			else
			{
				xenotype = XenotypeDefOf.Baseliner;
			}
		}
		if (ChildRelationUtility.DefinitelyHasNotBirthName(existingChild) || !(existingChild.Name is NameTriple nameTriple) || !ChildRelationUtility.ChildWantsNameOfAnyParent(existingChild))
		{
			return;
		}
		if (existingChild.GetMother() == null && existingChild.GetFather() == null)
		{
			if (Rand.Value < 0.5f)
			{
				lastName = nameTriple.Last;
			}
			return;
		}
		string last = nameTriple.Last;
		string text = null;
		if (existingChild.GetMother() != null && existingChild.GetMother().Name is NameTriple nameTriple2)
		{
			text = nameTriple2.Last;
		}
		else if (existingChild.GetFather() != null && existingChild.GetFather().Name is NameTriple nameTriple3)
		{
			text = nameTriple3.Last;
		}
		if (last != text)
		{
			lastName = last;
		}
	}

	private static void ResolveMyName(ref PawnGenerationRequest request, Pawn generated)
	{
		if (request.FixedLastName == null && ChildRelationUtility.ChildWantsNameOfAnyParent(generated))
		{
			if (Rand.Value < 0.5f && generated.GetFather().Name is NameTriple nameTriple)
			{
				request.SetFixedLastName(nameTriple.Last);
			}
			else if (generated.GetMother().Name is NameTriple nameTriple2)
			{
				request.SetFixedLastName(nameTriple2.Last);
			}
		}
	}
}
