using System.Linq;
using Verse;

namespace RimWorld;

public class GenStep_CrashedMechanoidPlatform : GenStep_LargeRuins
{
	private bool placedReactorLayout;

	private const float RuinAreaDirtFactor = 0.01f;

	private const float RuinAreaFilthFactor = 0.01f;

	private const float AreaCraterFactor = 0.01f;

	protected override IntRange RuinsMinMaxRange => new IntRange(1, 3);

	public override int SeedPart => 7937381;

	protected override int MoveRangeLimit => 6;

	protected override int ContractLimit => 6;

	protected override int MinRegionSize => 40;

	protected override LayoutDef LayoutDef => LayoutDefOf.CrashedMechanoidPlatform_Standard;

	protected override Faction Faction => Faction.OfMechanoids;

	protected override bool UseUsedRects => true;

	protected override TerrainAffordanceDef MinAffordance => TerrainAffordanceDefOf.Light;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckOdyssey("Crashed Mechanoid Platform"))
		{
			placedReactorLayout = false;
			base.Generate(map, parms);
		}
	}

	protected override LayoutStructureSketch GenerateAndSpawn(CellRect rect, Map map, GenStepParams parms, LayoutDef layoutDef)
	{
		if (!placedReactorLayout)
		{
			placedReactorLayout = true;
			layoutDef = LayoutDefOf.CrashedMechanoidPlatform_Engine;
		}
		LayoutStructureSketch layoutStructureSketch = base.GenerateAndSpawn(rect, map, parms, layoutDef);
		MapGenUtility.DoRectEdgeLumps(map, TerrainDefOf.MechanoidPlatform, ref rect, GenStep_OrbitalPlatform.LandingPadBorderLumpLengthRange, GenStep_OrbitalPlatform.LandingPadBorderLumpOffsetRange, TerrainDefOf.MechanoidPlatform);
		SpawnDirtDebrisCraters(rect, map, layoutStructureSketch);
		return layoutStructureSketch;
	}

	private static void SpawnDirtDebrisCraters(CellRect rect, Map map, LayoutStructureSketch sketch)
	{
		foreach (IntVec3 cell in rect.Cells)
		{
			if (!cell.GetTerrain(map).natural)
			{
				map.terrainGrid.SetTerrain(cell, TerrainDefOf.MechanoidPlatform);
				if (Rand.Range(0, 16) == 0)
				{
					ScatterDebrisUtility.ScatterFilthAroundCell(cell, new IntVec2(6, 6), map, Rand.Bool ? ThingDefOf.Filth_Ash : ThingDefOf.Filth_LooseGround);
				}
			}
		}
		foreach (IntVec3 item in rect.Cells.InRandomOrder().Take((int)((float)rect.Area * 0.01f)))
		{
			if (item.InBounds(map))
			{
				map.terrainGrid.SetTerrain(item, TerrainDefOf.PackedDirt);
			}
		}
		CellRect container = rect.ExpandedBy(8).ClipInsideMap(map);
		foreach (IntVec3 item2 in rect.DifferenceCells(container).InRandomOrder().Take((int)((float)container.Area * 0.01f)))
		{
			if (item2.InBounds(map))
			{
				ScatterDebrisUtility.ScatterFilthAroundCell(item2, new IntVec2(6, 6), map, Rand.Bool ? ThingDefOf.Filth_Ash : ThingDefOf.Filth_LooseGround);
			}
		}
		foreach (IntVec3 item3 in rect.ExpandedBy(4).ClipInsideMap(map).DifferenceCells(container)
			.InRandomOrder()
			.Take((int)((float)container.Area * 0.01f)))
		{
			GenSpawn.TrySpawn(Rand.Range(0, 3) switch
			{
				0 => ThingDefOf.CraterSmall, 
				1 => ThingDefOf.CraterMedium, 
				_ => ThingDefOf.CraterLarge, 
			}, item3, map, out var _, WipeMode.Vanish, canWipeEdifices: false);
		}
	}
}
