using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PsychicEmanatorSoothe : ThoughtWorker
	{
		private const float Radius = 15f;

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.Spawned)
			{
				return false;
			}
			List<Thing> list = p.Map.listerThings.ThingsOfDef(ThingDefOf.PsychicEmanator);
			for (int i = 0; i < list.Count; i++)
			{
				CompPowerTrader compPowerTrader = list[i].TryGetComp<CompPowerTrader>();
				if ((compPowerTrader == null || compPowerTrader.PowerOn) && p.Position.InHorDistOf(list[i].Position, 15f))
				{
					return true;
				}
			}
			return false;
		}
	}
}
