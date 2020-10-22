using Verse;

namespace RimWorld
{
	public abstract class RoyalTitleAwardWorker
	{
		public RoyalTitleDef def;

		public virtual void OnPreAward(Pawn pawn, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle)
		{
		}

		public abstract void DoAward(Pawn pawn, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle);
	}
}
