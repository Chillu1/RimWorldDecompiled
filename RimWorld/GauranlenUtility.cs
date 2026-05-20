using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class GauranlenUtility
	{
		public static bool CocoonAndPodCellValidator(IntVec3 c, Map map, ThingDef thingDefToSpawn = null)
		{
			if (thingDefToSpawn?.plant != null && thingDefToSpawn.CanEverPlantAt(c, map, out var _).Accepted)
			{
				return false;
			}
			if (!c.InBounds(map) || !c.Standable(map))
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.category == ThingCategory.Building || thingList[i].def.IsFrame)
				{
					return false;
				}
				if (thingList[i].def == ThingDefOf.Plant_TreeGauranlen || thingList[i].def == ThingDefOf.GaumakerCocoon || thingList[i].def == ThingDefOf.Plant_PodGauranlen)
				{
					return false;
				}
			}
			return true;
		}

		public static List<Thing> BuildingsAffectingConnectionStrengthAt(IntVec3 pos, Map map, CompProperties_TreeConnection props)
		{
			return map.listerArtificialBuildingsForMeditation.GetForCell(pos, props.radiusToBuildingForConnectionStrengthLoss);
		}

		public static IEnumerable<Thing> GetConnectionsAffectedByBuilding(Map map, ThingDef def, Faction faction, IntVec3 pos, Rot4 rotation)
		{
			if (!MeditationUtility.CountsAsArtificialBuilding(def, faction))
			{
				yield break;
			}
			foreach (Thing item in map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.DryadSpawner)))
			{
				CompTreeConnection compTreeConnection = item.TryGetComp<CompTreeConnection>();
				if (compTreeConnection != null && compTreeConnection.WillBeAffectedBy(def, faction, pos, rotation))
				{
					yield return item;
				}
			}
		}

		public static void DrawConnectionsAffectedByBuildingOverlay(Map map, ThingDef def, Faction faction, IntVec3 pos, Rot4 rotation)
		{
			_ = (CompProperties_TreeConnection)def.CompDefFor<CompTreeConnection>();
			int num = 0;
			foreach (Thing item in GetConnectionsAffectedByBuilding(map, def, faction, pos, rotation))
			{
				if (num++ > 10)
				{
					break;
				}
				GenAdj.OccupiedRect(pos, rotation, def.size);
				GenDraw.DrawLineBetween(GenThing.TrueCenter(pos, rotation, def.size, def.Altitude), item.TrueCenter(), SimpleColor.Red);
			}
		}
	}
}
