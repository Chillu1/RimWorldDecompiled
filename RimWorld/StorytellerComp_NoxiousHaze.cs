using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_NoxiousHaze : StorytellerComp
	{
		protected StorytellerCompProperties_NoxiousHaze Props => (StorytellerCompProperties_NoxiousHaze)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			Map map = (Map)target;
			float num = WorldPollutionUtility.CalculateNearbyPollutionScore(map.Tile);
			if (!(num < GameConditionDefOf.NoxiousHaze.minNearbyPollution))
			{
				float num2 = GameConditionDefOf.NoxiousHaze.mtbOverNearbyPollutionCurve.Evaluate(num);
				float num3 = -1f;
				if (map.storyState.lastFireTicks.ContainsKey(IncidentDefOf.NoxiousHaze))
				{
					num3 = (Find.TickManager.TicksGame - map.storyState.lastFireTicks[IncidentDefOf.NoxiousHaze]).TicksToDays();
				}
				if ((!(num3 >= 0f) || !(num3 < Props.lastFireMinMTBThreshold * num2)) && (num3 > Props.lastFireMaxMTBThreshold * num2 || Rand.MTBEventOccurs(num2, 60000f, 1000f)))
				{
					IncidentParms parms = GenerateParms(IncidentDefOf.NoxiousHaze.category, target);
					yield return new FiringIncident(IncidentDefOf.NoxiousHaze, this, parms);
				}
			}
		}
	}
}
