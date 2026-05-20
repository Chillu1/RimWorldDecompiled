using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_RequirementsToAcceptPlanetLayer : QuestPart_RequirementsToAccept
{
	public MapParent mapParent;

	public Pawn mapPawn;

	public bool canBeSpace;

	public List<PlanetLayerDef> layerWhitelist;

	public List<PlanetLayerDef> layerBlacklist;

	public override bool ShowInRequirementBox => false;

	public Map TargetMap
	{
		get
		{
			if (mapParent != null && mapParent.HasMap)
			{
				if (quest.IsParentSuitableForQuest(mapParent))
				{
					return mapParent.Map;
				}
				mapParent = quest.TryFindNewSuitableMapParentForRetarget();
				if (mapParent != null)
				{
					return mapParent.Map;
				}
			}
			return mapPawn?.MapHeld;
		}
	}

	public override AcceptanceReport CanAccept()
	{
		if (TargetMap == null || !TargetMap.Tile.Valid)
		{
			if (canBeSpace)
			{
				return true;
			}
			foreach (Map playerHomeMap in Current.Game.PlayerHomeMaps)
			{
				if (playerHomeMap.Tile.Valid && !playerHomeMap.Tile.LayerDef.isSpace)
				{
					return true;
				}
			}
			return "QuestNotSpace".Translate().CapitalizeFirst().Resolve();
		}
		PlanetLayerDef layerDef = TargetMap.Tile.LayerDef;
		if (!canBeSpace && layerDef.isSpace)
		{
			return GetReport(layerDef);
		}
		if (!layerWhitelist.NullOrEmpty() && !layerWhitelist.Contains(layerDef))
		{
			return GetReport(layerDef);
		}
		if (!layerBlacklist.NullOrEmpty() && layerBlacklist.Contains(layerDef))
		{
			return GetReport(layerDef);
		}
		return true;
	}

	private static AcceptanceReport GetReport(PlanetLayerDef def)
	{
		return "QuestRequiredLayer".Translate(def.gerundLabel.Named("GERUND"), def.label.Named("LAYER")).CapitalizeFirst().Resolve();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_References.Look(ref mapPawn, "mapPawn");
		Scribe_Values.Look(ref canBeSpace, "canBeSpace", defaultValue: false);
		Scribe_Collections.Look(ref layerWhitelist, "layerWhitelist", LookMode.Undefined);
		Scribe_Collections.Look(ref layerBlacklist, "layerBlacklist", LookMode.Undefined);
	}
}
