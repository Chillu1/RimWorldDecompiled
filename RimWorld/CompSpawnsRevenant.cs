using Verse;

namespace RimWorld;

public class CompSpawnsRevenant : ThingComp
{
	public int spawnTick = -1;

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref spawnTick, "spawnTick", 0);
	}

	public override void CompTickRare()
	{
		if (spawnTick > 0 && Find.TickManager.TicksGame > spawnTick && parent.MapHeld != null)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Revenant, Faction.OfEntities);
			GenSpawn.Spawn(pawn, parent.PositionHeld, parent.MapHeld);
			CompRevenant compRevenant = pawn.TryGetComp<CompRevenant>();
			compRevenant.Invisibility.BecomeVisible(instant: true);
			compRevenant.SetState(RevenantState.Escape);
			Find.LetterStack.ReceiveLetter("LetterLabelRevenantEmergence".Translate(), "LetterRevenantEmergence".Translate(), LetterDefOf.ThreatBig, pawn);
			parent.Destroy();
		}
	}
}
