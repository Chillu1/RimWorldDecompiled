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
					if (!TryFindRandomDoorSpawnCell(rp.rect, new Rot4(i), out var found))
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
					IntVec3 cell7 = new IntVec3(x, 0, rect.maxZ + 1);
					IntVec3 cell8 = new IntVec3(x, 0, rect.maxZ - 1);
					return CanPassThrough(cell7, map) && CanPassThrough(cell8, map);
				}, out var value))
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
					IntVec3 cell5 = new IntVec3(x, 0, rect.minZ - 1);
					IntVec3 cell6 = new IntVec3(x, 0, rect.minZ + 1);
					return CanPassThrough(cell5, map) && CanPassThrough(cell6, map);
				}, out var value2))
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
					IntVec3 cell3 = new IntVec3(rect.minX - 1, 0, z);
					IntVec3 cell4 = new IntVec3(rect.minX + 1, 0, z);
					return CanPassThrough(cell3, map) && CanPassThrough(cell4, map);
				}, out var value3))
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
				IntVec3 cell = new IntVec3(rect.maxX + 1, 0, z);
				IntVec3 cell2 = new IntVec3(rect.maxX - 1, 0, z);
				return CanPassThrough(cell, map) && CanPassThrough(cell2, map);
			}, out var value4))
			{
				found = IntVec3.Invalid;
				return false;
			}
			found = new IntVec3(rect.maxX, 0, value4);
			return true;
		}

		private bool CanPassThrough(IntVec3 cell, Map map)
		{
			if (cell.InBounds(map) && cell.Standable(map))
			{
				return cell.GetEdifice(map) == null;
			}
			return false;
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
