using Verse;

namespace RimWorld;

public class PawnRelationWorker_Parent : PawnRelationWorker
{
	private const float PlayerStartRelationFactor = 10f;

	public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
	{
		float num = 0f;
		if (!ChildRelationUtility.XenotypesCompatible(generated, other))
		{
			return 0f;
		}
		if (other.gender == Gender.Male)
		{
			num = ChildRelationUtility.ChanceOfBecomingChildOf(generated, other, other.GetFirstSpouseOfOppositeGender(), request, null, null);
		}
		else if (other.gender == Gender.Female)
		{
			num = ChildRelationUtility.ChanceOfBecomingChildOf(generated, other.GetFirstSpouseOfOppositeGender(), other, request, null, null);
		}
		if (ModsConfig.BiotechActive && request.Context == PawnGenerationContext.PlayerStarter && generated.DevelopmentalStage.Juvenile())
		{
			num *= 10f;
		}
		return num * BaseGenerationChanceFactor(generated, other, request);
	}

	public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
	{
		if (other.gender == Gender.Male)
		{
			generated.SetFather(other);
			Pawn firstSpouseOfOppositeGender = other.GetFirstSpouseOfOppositeGender();
			if (firstSpouseOfOppositeGender != null)
			{
				generated.SetMother(firstSpouseOfOppositeGender);
			}
			ResolveMyName(ref request, generated);
		}
		else if (other.gender == Gender.Female)
		{
			generated.SetMother(other);
			Pawn firstSpouseOfOppositeGender2 = other.GetFirstSpouseOfOppositeGender();
			if (firstSpouseOfOppositeGender2 != null)
			{
				generated.SetFather(firstSpouseOfOppositeGender2);
			}
			ResolveMyName(ref request, generated);
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
}
