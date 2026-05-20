using System.Collections.Generic;
using System.Linq;
using RimWorld.BaseGen;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientUtilityBuilding : SketchResolver
{
	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return parms.sketch != null;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (!ModLister.CheckIdeology("Ancient utility building"))
		{
			return;
		}
		ThingDef thingDef = BaseGenUtility.RandomCheapWallStuff(Faction.OfAncients, notVeryFlammable: true);
		LayoutSketch layoutSketch = new LayoutSketch
		{
			wallStuff = thingDef,
			doorStuff = thingDef
		};
		IntVec2 intVec = parms.utilityBuildingSize ?? new IntVec2(10, 10);
		StructureLayout structureLayout = (layoutSketch.structureLayout = RoomLayoutGenerator.GenerateRandomLayout(new CellRect(0, 0, intVec.x, intVec.z), 4, 4, 0f, canRemoveRooms: false, generateDoors: true, null, 2, IntRange.Zero));
		layoutSketch.FlushLayoutToSketch();
		List<LayoutRoom> rooms = structureLayout.Rooms;
		rooms.SortByDescending((LayoutRoom a) => a.Area);
		LayoutRoom layoutRoom = null;
		for (int num = 0; num < rooms.Count; num++)
		{
			if (!structureLayout.IsAdjacentToLayoutEdge(rooms[num]))
			{
				continue;
			}
			foreach (IntVec3 cell in rooms[num].Cells)
			{
				if (structureLayout.container.IsOnEdge(cell) && structureLayout.IsWallAt(cell))
				{
					layoutSketch.AddThing(ThingDefOf.AncientFence, cell, Rot4.North);
				}
			}
			IEnumerable<IntVec3> cellsToCheck = rooms[num].rects.SelectMany((CellRect r) => r.Cells).InRandomOrder();
			if (TryFindThingPositionWithGap(ThingDefOf.AncientGenerator, cellsToCheck, layoutSketch, out var position))
			{
				layoutSketch.AddThing(ThingDefOf.AncientGenerator, position, ThingDefOf.AncientGenerator.defaultPlacingRot);
			}
			foreach (CellRect rect in rooms[num].rects)
			{
				foreach (IntVec3 item in rect)
				{
					layoutSketch.AddTerrain(TerrainDefOf.AncientConcrete, item);
				}
			}
			layoutRoom = rooms[num];
			break;
		}
		for (int num2 = 0; num2 < rooms.Count; num2++)
		{
			if (rooms[num2] == layoutRoom)
			{
				continue;
			}
			foreach (IntVec3 item2 in rooms[num2].rects.SelectMany((CellRect r) => r.Cells))
			{
				layoutSketch.AddTerrain(TerrainDefOf.AncientConcrete, item2);
			}
		}
		parms.sketch.Merge(layoutSketch);
		SketchResolveParams parms2 = parms;
		parms2.wallEdgeThing = ThingDefOf.Table1x2c;
		parms2.requireFloor = true;
		parms2.allowWood = false;
		SketchResolverDefOf.AddWallEdgeThings.Resolve(parms2);
		SketchResolveParams parms3 = parms;
		parms3.destroyChanceExp = 1.5f;
		SketchResolverDefOf.DamageBuildings.Resolve(parms3);
	}

	private bool TryFindThingPositionWithGap(ThingDef thingDef, IEnumerable<IntVec3> cellsToCheck, Sketch sketch, out IntVec3 position, int gap = 1)
	{
		foreach (IntVec3 item in cellsToCheck)
		{
			CellRect cellRect = GenAdj.OccupiedRect(item, thingDef.defaultPlacingRot, thingDef.size).ExpandedBy(gap);
			bool flag = true;
			foreach (IntVec3 cell in cellRect.Cells)
			{
				if (sketch.EdificeAt(cell) != null)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				position = item;
				return true;
			}
		}
		position = IntVec3.Invalid;
		return false;
	}
}
