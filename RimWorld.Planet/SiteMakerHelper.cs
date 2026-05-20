using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public static class SiteMakerHelper
{
	private static List<Faction> possibleFactions = new List<Faction>();

	public static bool TryFindSiteParams_SingleSitePart(IEnumerable<SitePartDef> singleSitePartCandidates, out SitePartDef sitePart, out Faction faction, Faction factionToUse = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
	{
		faction = factionToUse;
		if (singleSitePartCandidates != null)
		{
			if (!TryFindNewRandomSitePartFor(null, singleSitePartCandidates, faction, out sitePart, disallowNonHostileFactions, extraFactionValidator))
			{
				return false;
			}
		}
		else
		{
			sitePart = null;
		}
		if (faction == null && !TryFindRandomFactionFor((sitePart != null) ? Gen.YieldSingle(sitePart) : null, out faction, disallowNonHostileFactions, extraFactionValidator))
		{
			return false;
		}
		return true;
	}

	public static bool TryFindSiteParams_SingleSitePart(string singleSitePartTag, out SitePartDef sitePart, out Faction faction, Faction factionToUse = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
	{
		return TryFindSiteParams_SingleSitePart(SitePartDefsWithTag(singleSitePartTag), out sitePart, out faction, factionToUse, disallowNonHostileFactions, extraFactionValidator);
	}

	public static bool TryFindSiteParams_MultipleSiteParts(IEnumerable<IEnumerable<SitePartDef>> sitePartsCandidates, out List<SitePartDef> siteParts, out Faction faction, Faction factionToUse = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
	{
		faction = factionToUse;
		siteParts = new List<SitePartDef>();
		if (sitePartsCandidates != null)
		{
			foreach (IEnumerable<SitePartDef> sitePartsCandidate in sitePartsCandidates)
			{
				if (sitePartsCandidate != null)
				{
					if (!TryFindNewRandomSitePartFor(siteParts, sitePartsCandidate, faction, out var sitePart, disallowNonHostileFactions, extraFactionValidator))
					{
						return false;
					}
					if (sitePart != null)
					{
						siteParts.Add(sitePart);
					}
				}
			}
		}
		if (faction == null && !TryFindRandomFactionFor(siteParts, out faction, disallowNonHostileFactions, extraFactionValidator))
		{
			return false;
		}
		return true;
	}

	public static bool TryFindSiteParams_MultipleSiteParts(IEnumerable<string> sitePartsTags, out List<SitePartDef> siteParts, out Faction faction, Faction factionToUse = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
	{
		return TryFindSiteParams_MultipleSiteParts(from x in sitePartsTags
			where x != null
			select SitePartDefsWithTag(x), out siteParts, out faction, factionToUse, disallowNonHostileFactions, extraFactionValidator);
	}

	public static bool TryFindNewRandomSitePartFor(IEnumerable<SitePartDef> existingSiteParts, IEnumerable<SitePartDef> possibleSiteParts, Faction faction, out SitePartDef sitePart, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
	{
		if (faction != null)
		{
			if (possibleSiteParts.Where((SitePartDef x) => x == null || (x.Worker.IsAvailable() && (existingSiteParts == null || existingSiteParts.All((SitePartDef p) => p != x && p.CompatibleWith(x))) && FactionCanOwn(x, faction, disallowNonHostileFactions, extraFactionValidator))).TryRandomElement(out sitePart))
			{
				return true;
			}
		}
		else
		{
			possibleFactions.Clear();
			possibleFactions.Add(null);
			possibleFactions.AddRange(Find.FactionManager.AllFactionsListForReading);
			if (possibleSiteParts.Where((SitePartDef x) => x == null || (x.Worker.IsAvailable() && (existingSiteParts == null || existingSiteParts.All((SitePartDef p) => p != x && p.CompatibleWith(x))) && possibleFactions.Any((Faction fac) => FactionCanOwn(existingSiteParts, fac, disallowNonHostileFactions, extraFactionValidator) && FactionCanOwn(x, fac, disallowNonHostileFactions, extraFactionValidator)))).TryRandomElement(out sitePart))
			{
				possibleFactions.Clear();
				return true;
			}
			possibleFactions.Clear();
		}
		sitePart = null;
		return false;
	}

	public static bool TryFindRandomFactionFor(IEnumerable<SitePartDef> parts, out Faction faction, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
	{
		if (FactionCanOwn(parts, null, disallowNonHostileFactions, extraFactionValidator))
		{
			faction = null;
			return true;
		}
		if (Find.FactionManager.AllFactionsListForReading.Where((Faction x) => FactionCanOwn(parts, x, disallowNonHostileFactions, extraFactionValidator)).TryRandomElement(out faction))
		{
			return true;
		}
		faction = null;
		return false;
	}

	public static void GenerateDefaultParams(float points, PlanetTile tile, Faction faction, IEnumerable<SitePartDef> siteParts, out List<SitePartDefWithParams> sitePartDefsWithParams)
	{
		int num = 0;
		if (siteParts != null)
		{
			foreach (SitePartDef sitePart in siteParts)
			{
				if (sitePart.wantsThreatPoints)
				{
					num++;
				}
			}
		}
		float num2 = ((num != 0) ? (points / (float)num) : 0f);
		if (siteParts != null)
		{
			List<SitePartDefWithParams> list = new List<SitePartDefWithParams>();
			foreach (SitePartDef sitePart2 in siteParts)
			{
				float myThreatPoints = (sitePart2.wantsThreatPoints ? num2 : 0f);
				list.Add(new SitePartDefWithParams(sitePart2, sitePart2.Worker.GenerateDefaultParams(myThreatPoints, tile, faction)));
			}
			sitePartDefsWithParams = list;
		}
		else
		{
			sitePartDefsWithParams = null;
		}
	}

	public static bool FactionCanOwn(IEnumerable<SitePartDef> parts, Faction faction, bool disallowNonHostileFactions, Predicate<Faction> extraFactionValidator)
	{
		if (parts != null)
		{
			foreach (SitePartDef part in parts)
			{
				if (!FactionCanOwn(part, faction, disallowNonHostileFactions, extraFactionValidator))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static IEnumerable<SitePartDef> SitePartDefsWithTag(string tag)
	{
		if (tag == null)
		{
			return null;
		}
		return DefDatabase<SitePartDef>.AllDefsListForReading.Where((SitePartDef x) => x.tags.Contains(tag));
	}

	private static bool FactionCanOwn(SitePartDef sitePart, Faction faction, bool disallowNonHostileFactions, Predicate<Faction> extraFactionValidator)
	{
		if (sitePart == null)
		{
			Log.Error("Called FactionCanOwn() with null SitePartDef.");
			return false;
		}
		if (faction != null && faction.temporary)
		{
			return false;
		}
		if (!sitePart.FactionCanOwn(faction))
		{
			return false;
		}
		if (disallowNonHostileFactions && faction != null && !faction.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		if (extraFactionValidator != null && !extraFactionValidator(faction))
		{
			return false;
		}
		return true;
	}
}
