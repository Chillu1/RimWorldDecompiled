using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class SitePartWorker_MechCluster : SitePartWorker
{
	public const float MinPoints = 750f;

	public override bool IsAvailable()
	{
		if (base.IsAvailable())
		{
			return Faction.OfMechanoids != null;
		}
		return false;
	}

	public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
	{
		string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
		lookTargets = new LookTargets(map.Parent);
		return arrivedLetterPart;
	}

	public override SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
	{
		SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
		sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, 750f);
		return sitePartParams;
	}
}
