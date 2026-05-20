using Verse;

namespace RimWorld;

public class RoyalTitleAwardWorker_Instant : RoyalTitleAwardWorker
{
	public override void DoAward(Pawn pawn, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle)
	{
		pawn.royalty.TryUpdateTitle(faction, sendLetter: true, def);
	}
}
