using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Parent : PawnRelationWorker
	{
		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			float num = 0f;
			if (other.gender == Gender.Male)
			{
				num = ChildRelationUtility.ChanceOfBecomingChildOf(generated, other, other.GetSpouseOppositeGender(), request, null, null);
			}
			else if (other.gender == Gender.Female)
			{
				num = ChildRelationUtility.ChanceOfBecomingChildOf(generated, other.GetSpouseOppositeGender(), other, request, null, null);
			}
			return num * BaseGenerationChanceFactor(generated, other, request);
		}

		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			if (other.gender == Gender.Male)
			{
				generated.SetFather(other);
				Pawn spouseOppositeGender = other.GetSpouseOppositeGender();
				if (spouseOppositeGender != null)
				{
					generated.SetMother(spouseOppositeGender);
				}
				ResolveMyName(ref request, generated);
				ResolveMySkinColor(ref request, generated);
			}
			else if (other.gender == Gender.Female)
			{
				generated.SetMother(other);
				Pawn spouseOppositeGender2 = other.GetSpouseOppositeGender();
				if (spouseOppositeGender2 != null)
				{
					generated.SetFather(spouseOppositeGender2);
				}
				ResolveMyName(ref request, generated);
				ResolveMySkinColor(ref request, generated);
			}
		}

		private static void ResolveMyName(ref PawnGenerationRequest request, Pawn generatedChild)
		{
			if (request.FixedLastName == null && ChildRelationUtility.ChildWantsNameOfAnyParent(generatedChild))
			{
				bool flag = Rand.Value < 0.5f || generatedChild.GetMother() == null;
				if (generatedChild.GetFather() == null)
				{
					flag = false;
				}
				if (flag)
				{
					request.SetFixedLastName(((NameTriple)generatedChild.GetFather().Name).Last);
				}
				else
				{
					request.SetFixedLastName(((NameTriple)generatedChild.GetMother().Name).Last);
				}
			}
		}

		private static void ResolveMySkinColor(ref PawnGenerationRequest request, Pawn generatedChild)
		{
			if (!request.FixedMelanin.HasValue)
			{
				if (generatedChild.GetFather() != null && generatedChild.GetMother() != null)
				{
					request.SetFixedMelanin(ChildRelationUtility.GetRandomChildSkinColor(generatedChild.GetFather().story.melanin, generatedChild.GetMother().story.melanin));
				}
				else if (generatedChild.GetFather() != null)
				{
					request.SetFixedMelanin(PawnSkinColors.GetRandomMelaninSimilarTo(generatedChild.GetFather().story.melanin));
				}
				else
				{
					request.SetFixedMelanin(PawnSkinColors.GetRandomMelaninSimilarTo(generatedChild.GetMother().story.melanin));
				}
			}
		}
	}
}
