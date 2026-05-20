using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_Gravship : SketchResolver
{
	private static readonly int[][] BaseStructure = new int[9][]
	{
		new int[10] { 0, 2, 2, 2, 3, 2, 2, 2, 2, 0 },
		new int[10] { 1, 1, 1, 1, 1, 1, 1, 1, 2, 0 },
		new int[10] { 2, 1, 1, 1, 1, 1, 1, 1, 2, 2 },
		new int[10] { 2, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
		new int[10] { 2, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
		new int[10] { 2, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
		new int[10] { 2, 1, 1, 1, 1, 1, 1, 1, 2, 2 },
		new int[10] { 1, 1, 1, 1, 1, 1, 1, 1, 2, 0 },
		new int[10] { 0, 2, 2, 2, 3, 2, 2, 2, 2, 0 }
	};

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return parms.sketch != null;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (ModLister.CheckOdyssey("Ancient launch pad"))
		{
			Sketch sketch = new Sketch();
			sketch.AddThing(ThingDefOf.GravEngine, new IntVec3(7, 0, 4), Rot4.North, null, 1, null, null, wipeIfCollides: true, 0.5f);
			GenerateBaseStructure(sketch);
			sketch.AddThing(ThingDefOf.ChemfuelTank, new IntVec3(6, 0, 1), Rot4.North);
			sketch.AddThing(ThingDefOf.ChemfuelTank, new IntVec3(6, 0, 6), Rot4.North);
			sketch.AddThing(ThingDefOf.SmallThruster, new IntVec3(0, 0, 1), Rot4.East);
			sketch.AddThing(ThingDefOf.SmallThruster, new IntVec3(0, 0, 7), Rot4.East);
			sketch.AddThing(ThingDefOf.PilotConsole, new IntVec3(4, 0, 4), Rot4.East);
			sketch.AddThing(ThingDefOf.Stool, new IntVec3(3, 0, 4), Rot4.East, ThingDefOf.WoodLog);
			sketch.AddThing(ThingDefOf.Shelf, new IntVec3(1, 0, 3), Rot4.East, ThingDefOf.Steel);
			sketch.AddThing(ThingDefOf.Shelf, new IntVec3(1, 0, 6), Rot4.East, ThingDefOf.Steel);
			sketch.AddThing(ThingDefOf.ShelfSmall, new IntVec3(1, 0, 4), Rot4.East, ThingDefOf.Steel);
			sketch.AddThing(ThingDefOf.Shelf, new IntVec3(2, 0, 1), Rot4.North, ThingDefOf.Steel);
			sketch.AddThing(ThingDefOf.Shelf, new IntVec3(3, 0, 7), Rot4.South, ThingDefOf.Steel);
			sketch.AddThing(ThingDefOf.Shelf, new IntVec3(5, 0, 1), Rot4.West, ThingDefOf.Steel);
			sketch.AddThing(ThingDefOf.Shelf, new IntVec3(5, 0, 6), Rot4.West, ThingDefOf.Steel);
			parms.sketch.Merge(sketch);
		}
	}

	private void GenerateBaseStructure(Sketch sketch)
	{
		for (int i = 0; i < BaseStructure.Length; i++)
		{
			int[] array = BaseStructure[i];
			for (int j = 0; j < array.Length; j++)
			{
				IntVec3 pos = new IntVec3(j, 0, i);
				switch (array[j])
				{
				case 1:
					sketch.AddTerrain(TerrainDefOf.Substructure, pos);
					break;
				case 2:
					sketch.AddTerrain(TerrainDefOf.Substructure, pos);
					sketch.AddThing(ThingDefOf.GravshipHull, pos, Rot4.North, ThingDefOf.Steel);
					break;
				case 3:
					sketch.AddTerrain(TerrainDefOf.Substructure, pos);
					sketch.AddThing(ThingDefOf.Door, pos, Rot4.North, ThingDefOf.Steel);
					break;
				}
			}
		}
	}
}
