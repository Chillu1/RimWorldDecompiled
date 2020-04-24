using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_ShuttleLandingBeaconUnusable : Alert
	{
		private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

		private List<GlobalTargetInfo> Targets
		{
			get
			{
				targets.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Thing> list = maps[i].listerThings.ThingsOfDef(ThingDefOf.ShipLandingBeacon);
					for (int j = 0; j < list.Count; j++)
					{
						CompShipLandingBeacon compShipLandingBeacon = list[j].TryGetComp<CompShipLandingBeacon>();
						if (compShipLandingBeacon != null && compShipLandingBeacon.Active && !compShipLandingBeacon.LandingAreas.Any())
						{
							targets.Add(list[j]);
						}
					}
				}
				return targets;
			}
		}

		public Alert_ShuttleLandingBeaconUnusable()
		{
			defaultLabel = "ShipLandingBeaconUnusable".Translate();
			defaultExplanation = "ShipLandingBeaconUnusableDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.RoyaltyActive)
			{
				return false;
			}
			return AlertReport.CulpritsAre(Targets);
		}
	}
}
