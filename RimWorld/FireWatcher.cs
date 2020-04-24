using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class FireWatcher
	{
		private Map map;

		private float fireDanger = -1f;

		private const int UpdateObservationsInterval = 426;

		private const float BaseDangerPerFire = 0.5f;

		public float FireDanger => fireDanger;

		public bool LargeFireDangerPresent
		{
			get
			{
				if (fireDanger < 0f)
				{
					UpdateObservations();
				}
				return fireDanger > 90f;
			}
		}

		public FireWatcher(Map map)
		{
			this.map = map;
		}

		public void FireWatcherTick()
		{
			if (Find.TickManager.TicksGame % 426 == 0)
			{
				UpdateObservations();
			}
		}

		private void UpdateObservations()
		{
			fireDanger = 0f;
			List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOf.Fire);
			for (int i = 0; i < list.Count; i++)
			{
				Fire fire = list[i] as Fire;
				fireDanger += 0.5f + fire.fireSize;
			}
		}
	}
}
