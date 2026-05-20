using Verse;

namespace RimWorld
{
	public class Alert_ChessTableNoChairs : Alert_JoyBuildingNoChairs
	{
		protected override JoyGiverDef JoyGiver => JoyGiverDefOf.Play_Chess;

		public Alert_ChessTableNoChairs()
		{
			defaultLabel = "ChessTablesNeedChairs".Translate();
			defaultExplanation = "ChessTablesNeedChairsDesc".Translate();
		}
	}
}
