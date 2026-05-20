using Verse;

namespace RimWorld;

public class ThoughtWorker_NoxiousHaze : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (NoxiousHazeUtility.IsExposedToNoxiousHaze(p) && AppliesTo(p))
		{
			return ThoughtState.ActiveDefault;
		}
		return ThoughtState.Inactive;
	}

	private bool AppliesTo(Pawn pawn)
	{
		if (ThoughtUtility.NullifyingGene(def, pawn) != null)
		{
			return true;
		}
		if (pawn.GetStatValue(StatDefOf.ToxicResistance) >= 1f)
		{
			return false;
		}
		if (pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance) >= 1f)
		{
			return false;
		}
		if (pawn.kindDef.immuneToGameConditionEffects)
		{
			return false;
		}
		return true;
	}
}
