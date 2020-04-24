using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_CategoryIndividualMTBByBiome : StorytellerComp
	{
		protected StorytellerCompProperties_CategoryIndividualMTBByBiome Props => (StorytellerCompProperties_CategoryIndividualMTBByBiome)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (target is World)
			{
				yield break;
			}
			List<IncidentDef> allIncidents = DefDatabase<IncidentDef>.AllDefsListForReading;
			for (int i = 0; i < allIncidents.Count; i++)
			{
				IncidentDef incidentDef = allIncidents[i];
				if (incidentDef.category != Props.category)
				{
					continue;
				}
				BiomeDef biome = Find.WorldGrid[target.Tile].biome;
				if (incidentDef.mtbDaysByBiome == null)
				{
					continue;
				}
				MTBByBiome mTBByBiome = incidentDef.mtbDaysByBiome.Find((MTBByBiome x) => x.biome == biome);
				if (mTBByBiome == null)
				{
					continue;
				}
				float num = mTBByBiome.mtbDays;
				if (Props.applyCaravanVisibility)
				{
					Caravan caravan = target as Caravan;
					if (caravan != null)
					{
						num /= caravan.Visibility;
					}
					else
					{
						Map map = target as Map;
						if (map != null && map.Parent.def.isTempIncidentMapOwner)
						{
							IEnumerable<Pawn> pawns = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Concat(map.mapPawns.PrisonersOfColonySpawned);
							num /= CaravanVisibilityCalculator.Visibility(pawns, caravanMovingNow: false);
						}
					}
				}
				if (Rand.MTBEventOccurs(num, 60000f, 1000f))
				{
					IncidentParms parms = GenerateParms(incidentDef.category, target);
					if (incidentDef.Worker.CanFireNow(parms))
					{
						yield return new FiringIncident(incidentDef, this, parms);
					}
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + Props.category;
		}
	}
}
