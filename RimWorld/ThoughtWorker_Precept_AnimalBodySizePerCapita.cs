using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_AnimalBodySizePerCapita : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
{
	private const float NoAnimals = 0f;

	private const float ScarceAnimals = 1f;

	private const float FewAnimals = 2f;

	private const float NoThought = 4f;

	private const float SomeAnimals = 6f;

	private const float LotsOfAnimals = 8f;

	private const int MinimumTicksBeforeFewAnimals = 900000;

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (ThoughtUtility.ThoughtNullified(p, def))
		{
			return false;
		}
		if (p.IsSlave)
		{
			return false;
		}
		float num = PawnUtility.PlayerAnimalBodySizePerCapita();
		if (num <= 2f && GenTicks.TicksAbs < 900000)
		{
			return false;
		}
		if (num < 4f && def.minExpectationForNegativeThought != null && p.MapHeld != null && ExpectationsUtility.CurrentExpectationFor(p.MapHeld).order < def.minExpectationForNegativeThought.order)
		{
			return false;
		}
		if (ThoughtStageFromAnimalDensity(num) < 0)
		{
			return false;
		}
		return ThoughtState.ActiveAtStage(ThoughtStageFromAnimalDensity(PawnUtility.PlayerAnimalBodySizePerCapita()));
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		return base.PostProcessDescription(p, description) + "\n\n" + "CurrentTotalAnimalBodySizePerColonist".Translate() + ": " + PawnUtility.PlayerAnimalBodySizePerCapita().ToString("F1") + "\n" + "MinAnimalBodySizePerColonist".Translate(4f.ToString("F1"));
	}

	private int ThoughtStageFromAnimalDensity(float density)
	{
		if (density < 0f)
		{
			return 0;
		}
		if (density < 1f)
		{
			return 1;
		}
		if (density < 2f)
		{
			return 2;
		}
		if (density < 4f)
		{
			return -1;
		}
		if (density < 6f)
		{
			return 3;
		}
		if (density < 8f)
		{
			return 4;
		}
		return 5;
	}

	public IEnumerable<NamedArgument> GetDescriptionArgs()
	{
		yield return string.Concat("(" + "AnimalsBodySizePerColonist".Translate() + ": ", 1f.ToString(), ")").Named("STAGE1");
		yield return string.Concat("(" + "AnimalsBodySizePerColonist".Translate() + ": ", 2f.ToString(), ")").Named("STAGE2");
		yield return string.Concat("(" + "AnimalsBodySizePerColonist".Translate() + ": ", 6f.ToString(), ")").Named("STAGE4");
		yield return string.Concat("(" + "AnimalsBodySizePerColonist".Translate() + ": ", 8f.ToString(), ")").Named("STAGE5");
	}
}
