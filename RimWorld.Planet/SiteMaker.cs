using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public static class SiteMaker
{
	public static Site MakeSite(SitePartDef sitePart, PlanetTile tile, Faction faction, bool ifHostileThenMustRemainHostile = true, float? threatPoints = null, WorldObjectDef worldObjectDef = null)
	{
		return MakeSite((sitePart != null) ? Gen.YieldSingle(sitePart) : null, tile, faction, ifHostileThenMustRemainHostile, threatPoints, worldObjectDef);
	}

	public static Site MakeSite(IEnumerable<SitePartDef> siteParts, PlanetTile tile, Faction faction, bool ifHostileThenMustRemainHostile = true, float? threatPoints = null, WorldObjectDef worldObjectDef = null)
	{
		float num = threatPoints ?? StorytellerUtility.DefaultSiteThreatPointsNow();
		SiteMakerHelper.GenerateDefaultParams(num, tile, faction, siteParts, out var sitePartDefsWithParams);
		Site site = MakeSite(sitePartDefsWithParams, tile, faction, ifHostileThenMustRemainHostile, worldObjectDef);
		site.desiredThreatPoints = num;
		return site;
	}

	public static Site MakeSite(IEnumerable<SitePartDefWithParams> siteParts, PlanetTile tile, Faction faction, bool ifHostileThenMustRemainHostile = true, WorldObjectDef worldObjectDef = null)
	{
		Site site = (Site)WorldObjectMaker.MakeWorldObject(worldObjectDef ?? WorldObjectDefOf.Site);
		site.Tile = tile;
		site.SetFaction(faction);
		if (ifHostileThenMustRemainHostile && faction != null && faction.HostileTo(Faction.OfPlayer))
		{
			site.factionMustRemainHostile = true;
		}
		if (siteParts != null)
		{
			foreach (SitePartDefWithParams sitePart in siteParts)
			{
				site.AddPart(new SitePart(site, sitePart.def, sitePart.parms));
			}
		}
		site.desiredThreatPoints = site.ActualThreatPoints;
		return site;
	}

	public static Site TryMakeSite_SingleSitePart(IEnumerable<SitePartDef> singleSitePartCandidates, PlanetTile tile, Faction faction = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null, bool ifHostileThenMustRemainHostile = true, float? threatPoints = null)
	{
		if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(singleSitePartCandidates, out var sitePart, out faction, faction, disallowNonHostileFactions, extraFactionValidator))
		{
			return null;
		}
		return MakeSite(sitePart, tile, faction, ifHostileThenMustRemainHostile, threatPoints);
	}

	public static Site TryMakeSite_SingleSitePart(string singleSitePartTag, PlanetTile tile, Faction faction = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null, bool ifHostileThenMustRemainHostile = true, float? threatPoints = null)
	{
		if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(singleSitePartTag, out var sitePart, out faction, faction, disallowNonHostileFactions, extraFactionValidator))
		{
			return null;
		}
		return MakeSite(sitePart, tile, faction, ifHostileThenMustRemainHostile, threatPoints);
	}

	public static Site TryMakeSite_MultipleSiteParts(IEnumerable<IEnumerable<SitePartDef>> sitePartsCandidates, PlanetTile tile, Faction faction = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null, bool ifHostileThenMustRemainHostile = true, float? threatPoints = null)
	{
		if (!SiteMakerHelper.TryFindSiteParams_MultipleSiteParts(sitePartsCandidates, out var siteParts, out faction, faction, disallowNonHostileFactions, extraFactionValidator))
		{
			return null;
		}
		return MakeSite(siteParts, tile, faction, ifHostileThenMustRemainHostile, threatPoints);
	}

	public static Site TryMakeSite_MultipleSiteParts(List<string> sitePartsTags, PlanetTile tile, Faction faction = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null, bool ifHostileThenMustRemainHostile = true, float? threatPoints = null)
	{
		if (!SiteMakerHelper.TryFindSiteParams_MultipleSiteParts(sitePartsTags, out var siteParts, out faction, faction, disallowNonHostileFactions, extraFactionValidator))
		{
			return null;
		}
		return MakeSite(siteParts, tile, faction, ifHostileThenMustRemainHostile, threatPoints);
	}

	public static Site TryMakeSite(IEnumerable<SitePartDef> siteParts, PlanetTile tile, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null, bool ifHostileThenMustRemainHostile = true, float? threatPoints = null)
	{
		if (!SiteMakerHelper.TryFindRandomFactionFor(siteParts, out var faction, disallowNonHostileFactions, extraFactionValidator))
		{
			return null;
		}
		return MakeSite(siteParts, tile, faction, ifHostileThenMustRemainHostile, threatPoints);
	}
}
