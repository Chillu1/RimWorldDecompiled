using Verse;

namespace RimWorld;

public class RoyalTitleAwardWorker
{
	public RoyalTitleDef def;

	public virtual void OnPreAward(Pawn pawn, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle)
	{
	}

	public virtual void DoAward(Pawn pawn, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle)
	{
	}
}
