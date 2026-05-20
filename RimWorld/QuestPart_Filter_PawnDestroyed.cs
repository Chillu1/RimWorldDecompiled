using Verse;

namespace RimWorld;

public class QuestPart_Filter_PawnDestroyed : QuestPart_Filter
{
	public Pawn pawn;

	protected override bool Pass(SignalArgs args)
	{
		return pawn.DestroyedOrNull();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref pawn, "pawn");
	}
}
