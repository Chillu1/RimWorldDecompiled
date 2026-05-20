using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_ForcedMap : ScenPart
{
	public MapGeneratorDef mapGenerator;

	public PlanetLayerDef layerDef;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref mapGenerator, "mapGenerator");
		Scribe_Defs.Look(ref layerDef, "layerDef");
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
		scenPartRect.height = ScenPart.RowHeight;
		Text.Anchor = TextAnchor.UpperRight;
		Rect rect = new Rect(scenPartRect.x - 200f, scenPartRect.y + ScenPart.RowHeight, 200f, ScenPart.RowHeight);
		rect.xMax -= 4f;
		Widgets.Label(rect, "ScenPart_ForcedMapPlanetLayer".Translate());
		Text.Anchor = TextAnchor.UpperLeft;
		if (Widgets.ButtonText(scenPartRect, mapGenerator.LabelCap))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (MapGeneratorDef item in DefDatabase<MapGeneratorDef>.AllDefs.Where((MapGeneratorDef d) => d.validScenarioMap))
			{
				MapGeneratorDef localFd = item;
				list.Add(new FloatMenuOption(localFd.LabelCap, delegate
				{
					mapGenerator = localFd;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		scenPartRect.y += ScenPart.RowHeight;
		if (!Widgets.ButtonText(scenPartRect, layerDef.LabelCap))
		{
			return;
		}
		List<FloatMenuOption> list2 = new List<FloatMenuOption>();
		foreach (PlanetLayerDef allDef in DefDatabase<PlanetLayerDef>.AllDefs)
		{
			PlanetLayerDef localFd2 = allDef;
			list2.Add(new FloatMenuOption(localFd2.LabelCap, delegate
			{
				layerDef = localFd2;
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list2));
	}

	public override void PostWorldGenerate()
	{
		PlanetTile planetTile = TileFinder.RandomStartingTile();
		PlanetLayer planetLayer = Find.WorldGrid.FirstLayerOfDef(layerDef);
		if (layerDef != PlanetLayerDefOf.Surface && planetLayer != null)
		{
			planetTile = planetLayer.GetClosestTile_NewTemp(planetTile, validSettlement: true);
		}
		Find.GameInitData.startingTile = planetTile;
		Find.GameInitData.mapGeneratorDef = mapGenerator;
	}
}
