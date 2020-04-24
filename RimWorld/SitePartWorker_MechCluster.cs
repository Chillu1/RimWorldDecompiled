using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SitePartWorker_MechCluster : SitePartWorker
	{
		public const float MinPoints = 750f;

		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
			List<Thing> list = new List<Thing>();
			foreach (Thing allThing in map.listerThings.AllThings)
			{
				if ((allThing.def.building != null && allThing.def.building.buildingTags != null && allThing.def.building.buildingTags.Contains("MechClusterMember")) || (allThing is Pawn && allThing.def.race.IsMechanoid))
				{
					list.Add(allThing);
				}
			}
			lookTargets = new LookTargets(list);
			return arrivedLetterPart;
		}

		public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
		{
			SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
			sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, 750f);
			return sitePartParams;
		}
	}
}
