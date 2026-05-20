using Verse;

namespace RimWorld
{
	public class CompRitualHediffGiverInRoom : ThingComp
	{
		private const int CheckInterval = 60;

		private CompProperties_RitualHediffGiverInRoom Props => (CompProperties_RitualHediffGiverInRoom)props;

		public override void CompTick()
		{
			if (!parent.Spawned || !parent.IsHashIntervalTick(60) || !parent.IsRitualTarget())
			{
				return;
			}
			Room room = parent.GetRoom();
			if (room == null || room.TouchesMapEdge)
			{
				return;
			}
			foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
			{
				if (item.RaceProps.IsFlesh && item.GetRoom() == room && parent.Position.InHorDistOf(item.Position, Props.minRadius))
				{
					Hediff hediff = HediffMaker.MakeHediff(Props.hediff, item);
					if (Props.severity > 0f)
					{
						hediff.Severity = Props.severity;
					}
					item.health.AddHediff(hediff);
					if (Props.resetLastRecreationalDrugTick && item.mindState != null)
					{
						item.mindState.lastTakeRecreationalDrugTick = Find.TickManager.TicksGame;
					}
				}
			}
		}
	}
}
