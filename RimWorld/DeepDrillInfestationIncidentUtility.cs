using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class DeepDrillInfestationIncidentUtility
	{
		public static void GetUsableDeepDrills(Map map, List<Thing> outDrills)
		{
			outDrills.Clear();
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.CreatesInfestations);
			Faction ofPlayer = Faction.OfPlayer;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Faction == ofPlayer && list[i].TryGetComp<CompCreatesInfestations>().CanCreateInfestationNow)
				{
					outDrills.Add(list[i]);
				}
			}
		}
	}
}
