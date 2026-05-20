using UnityEngine;
using Verse;

namespace RimWorld;

public class CompDeepScanner : CompScanner
{
	public new CompProperties_ScannerMineralsDeep Props => props as CompProperties_ScannerMineralsDeep;

	public override AcceptanceReport CanUseNow
	{
		get
		{
			if (!parent.Map.Biome.hasBedrock)
			{
				return "CannotUseScannerNoBedrock".Translate();
			}
			return base.CanUseNow;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && !parent.Map.Biome.hasBedrock)
		{
			Messages.Message("MessageGroundPenetratingScannerNoBedrock".Translate(parent.Named("THING")), parent, MessageTypeDefOf.NegativeEvent, historical: false);
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		if (ShouldShowDeepResourceOverlay())
		{
			parent.Map.deepResourceGrid.MarkForDraw();
		}
	}

	public bool ShouldShowDeepResourceOverlay()
	{
		if (powerComp != null)
		{
			return powerComp.PowerOn;
		}
		return false;
	}

	protected override void DoFind(Pawn worker)
	{
		Map map = parent.Map;
		if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => CanScatterAt(x, map), map, out var result))
		{
			Log.Error("Could not find a center cell for deep scanning lump generation!");
		}
		ThingDef thingDef = ChooseLumpThingDef();
		int numCells = Mathf.CeilToInt(thingDef.deepLumpSizeRange.RandomInRange);
		foreach (IntVec3 item in GridShapeMaker.IrregularLump(result, map, numCells))
		{
			if (CanScatterAt(item, map) && !item.InNoBuildEdgeArea(map))
			{
				map.deepResourceGrid.SetAt(item, thingDef, thingDef.deepCountPerCell);
			}
		}
		string key = ("LetterDeepScannerFoundLump".CanTranslate() ? "LetterDeepScannerFoundLump" : ((!"DeepScannerFoundLump".CanTranslate()) ? "LetterDeepScannerFoundLump" : "DeepScannerFoundLump"));
		Find.LetterStack.ReceiveLetter("LetterLabelDeepScannerFoundLump".Translate() + ": " + thingDef.LabelCap, key.Translate(thingDef.label, worker.Named("FINDER")), LetterDefOf.PositiveEvent, new LookTargets(result, map));
	}

	private bool CanScatterAt(IntVec3 pos, Map map)
	{
		int index = CellIndicesUtility.CellToIndex(pos, map.Size.x);
		TerrainDef terrainDef = map.terrainGrid.BaseTerrainAt(pos);
		if ((terrainDef != null && terrainDef.IsWater && terrainDef.passability == Traversability.Impassable) || !pos.GetAffordances(map).Contains(ThingDefOf.DeepDrill.terrainAffordanceNeeded))
		{
			return false;
		}
		return !map.deepResourceGrid.GetCellBool(index);
	}

	protected ThingDef ChooseLumpThingDef()
	{
		return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight((ThingDef def) => def.deepCommonality);
	}
}
