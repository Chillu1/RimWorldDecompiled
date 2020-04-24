using Verse;

namespace RimWorld
{
	public struct RoyalTitleInheritanceOutcome
	{
		public Pawn heir;

		public RoyalTitleDef heirCurrentTitle;

		public bool heirTitleHigher;

		public bool FoundHeir => heir != null;

		public bool HeirHasTitle => heirCurrentTitle != null;
	}
}
