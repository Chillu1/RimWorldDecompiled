using Verse;

namespace RimWorld
{
	public class StrengthWatcher
	{
		private Map map;

		public float StrengthRating
		{
			get
			{
				float num = 0f;
				foreach (Pawn freeColonist in map.mapPawns.FreeColonists)
				{
					float num2 = 1f;
					num2 *= freeColonist.health.summaryHealth.SummaryHealthPercent;
					if (freeColonist.Downed)
					{
						num2 *= 0.3f;
					}
					num += num2;
				}
				foreach (Building allBuildingsColonistCombatTarget in map.listerBuildings.allBuildingsColonistCombatTargets)
				{
					if (allBuildingsColonistCombatTarget.def.building != null && allBuildingsColonistCombatTarget.def.building.IsTurret)
					{
						num += 0.3f;
					}
				}
				return num;
			}
		}

		public StrengthWatcher(Map map)
		{
			this.map = map;
		}
	}
}
