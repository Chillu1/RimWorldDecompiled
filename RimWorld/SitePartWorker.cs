using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class SitePartWorker
{
	public SitePartDef def;

	public virtual void SitePartWorkerTick(SitePart sitePart)
	{
	}

	public virtual void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		outExtraDescriptionRules.AddRange(GrammarUtility.RulesForDef("", part.def));
		outExtraDescriptionConstants.Add("sitePart", part.def.defName);
	}

	public virtual void PostMapGenerate(Map map)
	{
	}

	public virtual bool FactionCanOwn(Faction faction)
	{
		return true;
	}

	public virtual string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
	{
		preferredLetterDef = def.arrivedLetterDef;
		lookTargets = new LookTargets(map.Parent);
		return def.arrivedLetter;
	}

	public virtual string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		return def.label;
	}

	public virtual SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
	{
		return new SitePartParams
		{
			randomValue = Rand.Int,
			threatPoints = (def.wantsThreatPoints ? myThreatPoints : 0f)
		};
	}

	public virtual bool IncreasesPopulation(SitePartParams parms)
	{
		return def.increasesPopulation;
	}

	public virtual void Init(Site site, SitePart sitePart)
	{
	}

	public virtual bool IsAvailable()
	{
		return true;
	}

	public virtual void PostDrawExtraSelectionOverlays(SitePart sitePart)
	{
	}

	public virtual void PostDestroy(SitePart sitePart)
	{
		if (def.leaveAbandonedSettlement && sitePart.site.Tile.LayerDef == PlanetLayerDefOf.Surface)
		{
			WorldObject worldObject = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.AbandonedSettlement);
			worldObject.Tile = sitePart.site.Tile;
			worldObject.SetFaction(Faction.OfPlayer);
			Find.WorldObjects.Add(worldObject);
		}
	}

	public virtual void Notify_SiteMapAboutToBeRemoved(SitePart sitePart)
	{
	}
}
