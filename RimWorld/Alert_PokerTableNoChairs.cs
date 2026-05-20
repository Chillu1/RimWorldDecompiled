using Verse;

namespace RimWorld
{
	public class Alert_PokerTableNoChairs : Alert_JoyBuildingNoChairs
	{
		protected override JoyGiverDef JoyGiver => JoyGiverDefOf.Play_Poker;

		public Alert_PokerTableNoChairs()
		{
			defaultLabel = "PokerTablesNeedChairs".Translate();
			defaultExplanation = "PokerTablesNeedChairsDesc".Translate();
		}
	}
}
