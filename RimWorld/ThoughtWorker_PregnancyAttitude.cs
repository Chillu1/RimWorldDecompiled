using Verse;

namespace RimWorld;

public class ThoughtWorker_PregnancyAttitude : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		if (!(PregnancyUtility.GetPregnancyHediff(p) is Hediff_Pregnant { CurStageIndex: <=0, Attitude: var attitude }))
		{
			return ThoughtState.Inactive;
		}
		if (attitude.HasValue)
		{
			switch (attitude.Value)
			{
			case PregnancyAttitude.Positive:
				return ThoughtState.ActiveAtStage(0);
			case PregnancyAttitude.Negative:
				return ThoughtState.ActiveAtStage(1);
			}
		}
		return ThoughtState.Inactive;
	}
}
