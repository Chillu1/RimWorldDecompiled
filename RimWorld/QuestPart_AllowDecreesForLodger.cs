using Verse;

namespace RimWorld;

public class QuestPart_AllowDecreesForLodger : QuestPart
{
	public Pawn lodger;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref lodger, "lodger");
	}
}
