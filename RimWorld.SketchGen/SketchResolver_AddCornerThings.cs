using System.Collections.Generic;
using Verse;

namespace RimWorld.SketchGen
{
	public class SketchResolver_AddCornerThings : SketchResolver
	{
		private HashSet<IntVec3> wallPositions = new HashSet<IntVec3>();

		private const float Chance = 0.09f;

		protected override void ResolveInt(ResolveParams parms)
		{
			wallPositions.Clear();
			for (int i = 0; i < parms.sketch.Things.Count; i++)
			{
				if (parms.sketch.Things[i].def == ThingDefOf.Wall)
				{
					wallPositions.Add(parms.sketch.Things[i].pos);
				}
			}
			bool allowWood = parms.allowWood ?? true;
			ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(parms.cornerThing, null, (ThingDef x) => SketchGenUtility.IsStuffAllowed(x, allowWood, parms.useOnlyStonesAvailableOnMap, allowFlammableWalls: true, parms.cornerThing));
			bool flag = parms.requireFloor ?? false;
			try
			{
				foreach (IntVec3 wallPosition in wallPositions)
				{
					if (Rand.Chance(0.09f))
					{
						if (wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z - 1)) && !wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)) && (!flag || (parms.sketch.TerrainAt(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)) != null && parms.sketch.TerrainAt(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)).layerable)))
						{
							parms.sketch.AddThing(parms.cornerThing, new IntVec3(wallPosition.x + 1, 0, wallPosition.z), Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
						}
						if (wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z - 1)) && !wallPositions.Contains(new IntVec3(wallPosition.x, 0, wallPosition.z - 1)) && (!flag || (parms.sketch.TerrainAt(new IntVec3(wallPosition.x, 0, wallPosition.z - 1)) != null && parms.sketch.TerrainAt(new IntVec3(wallPosition.x, 0, wallPosition.z - 1)).layerable)))
						{
							parms.sketch.AddThing(parms.cornerThing, new IntVec3(wallPosition.x, 0, wallPosition.z - 1), Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
						}
						if (wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z + 1)) && !wallPositions.Contains(new IntVec3(wallPosition.x, 0, wallPosition.z + 1)) && (!flag || (parms.sketch.TerrainAt(new IntVec3(wallPosition.x, 0, wallPosition.z + 1)) != null && parms.sketch.TerrainAt(new IntVec3(wallPosition.x, 0, wallPosition.z + 1)).layerable)))
						{
							parms.sketch.AddThing(parms.cornerThing, new IntVec3(wallPosition.x, 0, wallPosition.z + 1), Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
						}
						if (wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z + 1)) && !wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)) && (!flag || (parms.sketch.TerrainAt(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)) != null && parms.sketch.TerrainAt(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)).layerable)))
						{
							parms.sketch.AddThing(parms.cornerThing, new IntVec3(wallPosition.x + 1, 0, wallPosition.z), Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
						}
					}
				}
			}
			finally
			{
				wallPositions.Clear();
			}
		}

		protected override bool CanResolveInt(ResolveParams parms)
		{
			return true;
		}
	}
}
