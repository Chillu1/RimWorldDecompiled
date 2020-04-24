using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_ExtraDoor : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			IntVec3 intVec = IntVec3.Invalid;
			int num = -1;
			for (int i = 0; i < 4; i++)
			{
				if (WallHasDoor(rp.rect, new Rot4(i)))
				{
					continue;
				}
				for (int j = 0; j < 2; j++)
				{
					if (!TryFindRandomDoorSpawnCell(rp.rect, new Rot4(i), out IntVec3 found))
					{
						continue;
					}
					int distanceToExistingDoors = GetDistanceToExistingDoors(found, rp.rect);
					if (!intVec.IsValid || distanceToExistingDoors > num)
					{
						intVec = found;
						num = distanceToExistingDoors;
						if (num == int.MaxValue)
						{
							break;
						}
					}
				}
			}
			if (intVec.IsValid)
			{
				ThingDef stuff = rp.wallStuff ?? BaseGenUtility.WallStuffAt(intVec, map) ?? BaseGenUtility.RandomCheapWallStuff(rp.faction);
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Door, stuff);
				thing.SetFaction(rp.faction);
				GenSpawn.Spawn(thing, intVec, BaseGen.globalSettings.map);
			}
		}

		private bool WallHasDoor(CellRect rect, Rot4 dir)
		{
			Map map = BaseGen.globalSettings.map;
			foreach (IntVec3 edgeCell in rect.GetEdgeCells(dir))
			{
				if (edgeCell.GetDoor(map) != null)
				{
					return true;
				}
			}
			return false;
		}

		private bool TryFindRandomDoorSpawnCell(CellRect rect, Rot4 dir, out IntVec3 found)
		{
			Map map = BaseGen.globalSettings.map;
			if (dir == Rot4.North)
			{
				if (rect.Width <= 2)
				{
					found = IntVec3.Invalid;
					return false;
				}
				if (!Rand.TryRangeInclusiveWhere(rect.minX + 1, rect.maxX - 1, delegate(int x)
				{
					IntVec3 c7 = new IntVec3(x, 0, rect.maxZ + 1);
					IntVec3 c8 = new IntVec3(x, 0, rect.maxZ - 1);
					return c7.InBounds(map) && c7.Standable(map) && c8.InBounds(map) && c8.Standable(map);
				}, out int value))
				{
					found = IntVec3.Invalid;
					return false;
				}
				found = new IntVec3(value, 0, rect.maxZ);
				return true;
			}
			if (dir == Rot4.South)
			{
				if (rect.Width <= 2)
				{
					found = IntVec3.Invalid;
					return false;
				}
				if (!Rand.TryRangeInclusiveWhere(rect.minX + 1, rect.maxX - 1, delegate(int x)
				{
					IntVec3 c5 = new IntVec3(x, 0, rect.minZ - 1);
					IntVec3 c6 = new IntVec3(x, 0, rect.minZ + 1);
					return c5.InBounds(map) && c5.Standable(map) && c6.InBounds(map) && c6.Standable(map);
				}, out int value2))
				{
					found = IntVec3.Invalid;
					return false;
				}
				found = new IntVec3(value2, 0, rect.minZ);
				return true;
			}
			if (dir == Rot4.West)
			{
				if (rect.Height <= 2)
				{
					found = IntVec3.Invalid;
					return false;
				}
				if (!Rand.TryRangeInclusiveWhere(rect.minZ + 1, rect.maxZ - 1, delegate(int z)
				{
					IntVec3 c3 = new IntVec3(rect.minX - 1, 0, z);
					IntVec3 c4 = new IntVec3(rect.minX + 1, 0, z);
					return c3.InBounds(map) && c3.Standable(map) && c4.InBounds(map) && c4.Standable(map);
				}, out int value3))
				{
					found = IntVec3.Invalid;
					return false;
				}
				found = new IntVec3(rect.minX, 0, value3);
				return true;
			}
			if (rect.Height <= 2)
			{
				found = IntVec3.Invalid;
				return false;
			}
			if (!Rand.TryRangeInclusiveWhere(rect.minZ + 1, rect.maxZ - 1, delegate(int z)
			{
				IntVec3 c = new IntVec3(rect.maxX + 1, 0, z);
				IntVec3 c2 = new IntVec3(rect.maxX - 1, 0, z);
				return c.InBounds(map) && c.Standable(map) && c2.InBounds(map) && c2.Standable(map);
			}, out int value4))
			{
				found = IntVec3.Invalid;
				return false;
			}
			found = new IntVec3(rect.maxX, 0, value4);
			return true;
		}

		private int GetDistanceToExistingDoors(IntVec3 cell, CellRect rect)
		{
			Map map = BaseGen.globalSettings.map;
			int num = int.MaxValue;
			foreach (IntVec3 edgeCell in rect.EdgeCells)
			{
				if (edgeCell.GetDoor(map) != null)
				{
					num = Mathf.Min(num, Mathf.Abs(cell.x - edgeCell.x) + Mathf.Abs(cell.z - edgeCell.z));
				}
			}
			return num;
		}
	}
}
