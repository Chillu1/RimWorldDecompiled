using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_VeneratedAnimalOnMapOrCaravan : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
{
	private const float FewAnimals = 1f;

	private const float SomeAnimals = 2f;

	private const float ManyAnimals = 4f;

	private const float LotsOfAnimals = 6f;

	public override string PostProcessLabel(Pawn p, string label)
	{
		Pawn pawn = PawnUtility.FirstVeneratedAnimalOnMapOrCaravan(p);
		if (pawn == null)
		{
			return label;
		}
		return label.Formatted(NamedArgumentUtility.Named(pawn.kindDef, "ANIMAL"));
	}

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (p.IsSlave)
		{
			return false;
		}
		int num = ThoughtStageFromAnimalDensity(PawnUtility.PlayerVeneratedAnimalBodySizePerCapitaOnMapOrCaravan(p));
		if (num >= 0)
		{
			return ThoughtState.ActiveAtStage(num);
		}
		return false;
	}

	public IEnumerable<NamedArgument> GetDescriptionArgs()
	{
		yield return string.Concat("(" + "VeneratedAnimalsBodySizePerColonist".Translate() + ": ", 1f.ToString(), ")").Named("STAGE1");
		yield return string.Concat("(" + "VeneratedAnimalsBodySizePerColonist".Translate() + ": ", 2f.ToString(), ")").Named("STAGE2");
		yield return string.Concat("(" + "VeneratedAnimalsBodySizePerColonist".Translate() + ": ", 4f.ToString(), ")").Named("STAGE3");
		yield return string.Concat("(" + "VeneratedAnimalsBodySizePerColonist".Translate() + ": ", 6f.ToString(), ")").Named("STAGE4");
	}

	private int ThoughtStageFromAnimalDensity(float density)
	{
		if (density <= 0f)
		{
			return -1;
		}
		if (density < 1f)
		{
			return 0;
		}
		if (density < 2f)
		{
			return 1;
		}
		if (density < 4f)
		{
			return 2;
		}
		if (density < 6f)
		{
			return 3;
		}
		return 4;
	}
}
