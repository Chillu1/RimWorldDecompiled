using Verse;

namespace RimWorld;

public class PawnRelationWorker_Child : PawnRelationWorker
{
	private const float PlayerStartRelationFactor = 10f;

	public override bool InRelation(Pawn me, Pawn other)
	{
		if (me == other)
		{
			return false;
		}
		if (other.GetMother() == me || other.GetFather() == me)
		{
			return true;
		}
		if (ModsConfig.BiotechActive && other.GetBirthParent() == me)
		{
			return true;
		}
		return false;
	}

	public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
	{
		float num = 0f;
		if (!ChildRelationUtility.XenotypesCompatible(generated, other))
		{
			return 0f;
		}
		if (other.IsDuplicate)
		{
			return 0f;
		}
		if (generated.gender == Gender.Male)
		{
			num = ChildRelationUtility.ChanceOfBecomingChildOf(other, generated, other.GetMother(), null, request, null);
		}
		else if (generated.gender == Gender.Female)
		{
			num = ChildRelationUtility.ChanceOfBecomingChildOf(other, other.GetFather(), generated, null, null, request);
		}
		if (ModsConfig.BiotechActive && request.Context == PawnGenerationContext.PlayerStarter && other.DevelopmentalStage.Juvenile())
		{
			num *= 10f;
		}
		return num * BaseGenerationChanceFactor(generated, other, request);
	}

	public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
	{
		if (generated.gender == Gender.Male)
		{
			other.SetFather(generated);
			ResolveMyName(ref request, other, other.GetMother());
			if (other.GetMother() != null)
			{
				if (other.GetMother().story.traits.HasTrait(TraitDefOf.Gay))
				{
					generated.relations.AddDirectRelation(PawnRelationDefOf.ExLover, other.GetMother());
				}
				else if (Rand.Value < 0.85f && !LovePartnerRelationUtility.HasAnyLovePartner(other.GetMother()))
				{
					generated.relations.AddDirectRelation(PawnRelationDefOf.Spouse, other.GetMother());
					SpouseRelationUtility.ResolveNameForSpouseOnGeneration(ref request, generated);
				}
				else
				{
					LovePartnerRelationUtility.GiveRandomExLoverOrExSpouseRelation(generated, other.GetMother());
				}
			}
		}
		else
		{
			if (generated.gender != Gender.Female)
			{
				return;
			}
			other.SetMother(generated);
			ResolveMyName(ref request, other, other.GetFather());
			if (other.GetFather() != null)
			{
				if (other.GetFather().story.traits.HasTrait(TraitDefOf.Gay))
				{
					generated.relations.AddDirectRelation(PawnRelationDefOf.ExLover, other.GetFather());
				}
				else if (Rand.Value < 0.85f && !LovePartnerRelationUtility.HasAnyLovePartner(other.GetFather()))
				{
					generated.relations.AddDirectRelation(PawnRelationDefOf.Spouse, other.GetFather());
					SpouseRelationUtility.ResolveNameForSpouseOnGeneration(ref request, generated);
				}
				else
				{
					LovePartnerRelationUtility.GiveRandomExLoverOrExSpouseRelation(generated, other.GetFather());
				}
			}
		}
	}

	private static void ResolveMyName(ref PawnGenerationRequest request, Pawn child, Pawn otherParent)
	{
		if (request.FixedLastName != null || ChildRelationUtility.DefinitelyHasNotBirthName(child) || !ChildRelationUtility.ChildWantsNameOfAnyParent(child))
		{
			return;
		}
		if (otherParent == null || !(otherParent.Name is NameTriple))
		{
			float num = 0.875f;
			if (Rand.Value < num)
			{
				request.SetFixedLastName(((NameTriple)child.Name).Last);
			}
			return;
		}
		string last = ((NameTriple)child.Name).Last;
		string last2 = ((NameTriple)otherParent.Name).Last;
		if (last != last2)
		{
			request.SetFixedLastName(last);
		}
	}
}
