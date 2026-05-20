using Verse;

namespace RimWorld;

public class ThoughtWorker_ReadingTome : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (p.CurJobDef != JobDefOf.Reading)
		{
			return false;
		}
		return p.CurJob.targetA.Thing.def == ThingDefOf.Tome;
	}
}
