using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompCauseGameCondition_ToxicSpewer : CompCauseGameCondition
	{
		public override void CompTick()
		{
			base.CompTick();
			if (!base.Active || Find.TickManager.TicksGame % 3451 != 0)
			{
				return;
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				if (Find.WorldGrid.ApproxDistanceInTiles(caravans[i].Tile, base.MyTile) < (float)base.Props.worldRange)
				{
					List<Pawn> pawnsListForReading = caravans[i].PawnsListForReading;
					for (int j = 0; j < pawnsListForReading.Count; j++)
					{
						GameCondition_ToxicFallout.DoPawnToxicDamage(pawnsListForReading[j]);
					}
				}
			}
		}
	}
}
