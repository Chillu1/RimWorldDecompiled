using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Child : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			if (other.GetMother() != me)
			{
				return other.GetFather() == me;
			}
			return true;
		}

		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			float num = 0f;
			if (generated.gender == Gender.Male)
			{
				num = ChildRelationUtility.ChanceOfBecomingChildOf(other, generated, other.GetMother(), null, request, null);
			}
			else if (generated.gender == Gender.Female)
			{
				num = ChildRelationUtility.ChanceOfBecomingChildOf(other, other.GetFather(), generated, null, null, request);
			}
			return num * BaseGenerationChanceFactor(generated, other, request);
		}

		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			if (generated.gender == Gender.Male)
			{
				other.SetFather(generated);
				ResolveMyName(ref request, other, other.GetMother());
				ResolveMySkinColor(ref request, other, other.GetMother());
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
				ResolveMySkinColor(ref request, other, other.GetFather());
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
			if (otherParent == null)
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

		private static void ResolveMySkinColor(ref PawnGenerationRequest request, Pawn child, Pawn otherParent)
		{
			if (!request.FixedMelanin.HasValue)
			{
				if (otherParent != null)
				{
					request.SetFixedMelanin(ParentRelationUtility.GetRandomSecondParentSkinColor(otherParent.story.melanin, child.story.melanin));
				}
				else
				{
					request.SetFixedMelanin(PawnSkinColors.GetRandomMelaninSimilarTo(child.story.melanin));
				}
			}
		}
	}
}
