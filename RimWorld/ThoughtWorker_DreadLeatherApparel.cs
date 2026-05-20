using Verse;

namespace RimWorld;

public class ThoughtWorker_DreadLeatherApparel : ThoughtWorker_ApparelThought
{
	protected override bool ApparelCounts(Apparel apparel)
	{
		return apparel.Stuff == ThingDefOf.Leather_Dread;
	}

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		return base.CurrentStateInternal(p);
	}
}
