namespace RimWorld;

public class ThoughtWorker_DeadMansApparel : ThoughtWorker_ApparelThought
{
	protected override bool ApparelCounts(Apparel apparel)
	{
		return apparel.WornByCorpse;
	}
}
