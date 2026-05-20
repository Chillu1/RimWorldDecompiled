using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class GenStep_FrozenRuins : GenStep_BaseRuins
{
	private const int CellRange = 24;

	public override int SeedPart => 964521;

	protected override int RegionSize => 25;

	protected override FloatRange DefaultMapFillPercentRange => new FloatRange(0.01f, 0.05f);

	protected override FloatRange MergeRange => new FloatRange(0.05f, 0.15f);

	protected override int MoveRangeLimit => 4;

	protected override int ContractLimit => 4;

	protected override int MinRegionSize => 14;

	protected override LayoutDef LayoutDef => LayoutDefOf.AncientRuinsGlacier;

	protected override Faction Faction => Faction.OfAncientsHostile;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckOdyssey("Frozen Ruins"))
		{
			base.Generate(map, parms);
		}
	}

	public override void GenerateRuins(Map map, GenStepParams parms, FloatRange mapFillPercentRange)
	{
		base.GenerateRuins(map, parms, mapFillPercentRange);
		EncaseStructuresInIce(map, structureSketches);
	}

	public static void EncaseStructuresInIce(Map map, List<LayoutStructureSketch> structureSketches, GenMorphology.GenPatchParms? patchParms = null)
	{
		foreach (LayoutStructureSketch structureSketch in structureSketches)
		{
			if (structureSketch.structureLayout == null)
			{
				continue;
			}
			List<CellRect> rects = (from r in structureSketch.structureLayout.Rooms.SelectMany((LayoutRoom room) => room.rects)
				select r.ExpandedBy(24)).ToList();
			GenMorphology.GenPatchParms valueOrDefault = patchParms.GetValueOrDefault();
			if (!patchParms.HasValue)
			{
				valueOrDefault = GenMorphology.GenPatchParms.For(ThingDefOf.SolidIce, TerrainDefOf.Ice);
				patchParms = valueOrDefault;
			}
			GenMorphology.GenerateNaturalPatch(map, rects, patchParms, Validator);
			foreach (LayoutRoom room in structureSketch.structureLayout.Rooms)
			{
				foreach (IntVec3 cell in room.Cells)
				{
					if (map.roofGrid.RoofAt(cell) == null)
					{
						map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThin);
					}
				}
			}
		}
		bool Validator(IntVec3 cell)
		{
			if (cell.GetEdifice(map) != null)
			{
				return false;
			}
			if (cell.GetTerrain(map).IsWater)
			{
				return false;
			}
			return !structureSketches.Any((LayoutStructureSketch s) => s.AnyRoomContains(cell));
		}
	}

	protected override bool IsValidRect(CellRect rect, Map map)
	{
		if (!base.IsValidRect(rect, map))
		{
			return false;
		}
		if (map.TileInfo.Mutators.Contains(TileMutatorDefOf.Crevasse))
		{
			foreach (IntVec3 cell in rect.Cells)
			{
				if (cell.GetEdifice(map)?.def != ThingDefOf.SolidIce)
				{
					return false;
				}
			}
			return true;
		}
		return true;
	}
}
