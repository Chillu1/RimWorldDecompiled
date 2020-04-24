using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Outdoors_Division_Grid : SymbolResolver
	{
		private class Child
		{
			public CellRect rect;

			public int gridX;

			public int gridY;

			public bool merged;
		}

		private List<Pair<int, int>> optionsX = new List<Pair<int, int>>();

		private List<Pair<int, int>> optionsZ = new List<Pair<int, int>>();

		private List<Child> children = new List<Child>();

		private const int MinWidthOrHeight = 13;

		private const int MinRoomsPerRow = 2;

		private const int MaxRoomsPerRow = 4;

		private const int MinPathwayWidth = 1;

		private const int MaxPathwayWidth = 5;

		private const int MinRoomSize = 6;

		private const float AllowNonSquareRoomsInTheFirstStepChance = 0.2f;

		private static List<Pair<Pair<int, int>, Pair<int, int>>> options = new List<Pair<Pair<int, int>, Pair<int, int>>>();

		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			if (rp.rect.Width < 13 && rp.rect.Height < 13)
			{
				return false;
			}
			FillOptions(rp.rect);
			if (optionsX.Any())
			{
				return optionsZ.Any();
			}
			return false;
		}

		public override void Resolve(ResolveParams rp)
		{
			FillOptions(rp.rect);
			if ((Rand.Chance(0.2f) || (!TryResolveRandomOption(0, 0, rp) && !TryResolveRandomOption(0, 1, rp))) && !TryResolveRandomOption(1, 0, rp) && !TryResolveRandomOption(2, 0, rp) && !TryResolveRandomOption(2, 1, rp) && !TryResolveRandomOption(999999, 999999, rp))
			{
				Log.Warning("Grid resolver could not resolve any grid size. params=" + rp);
			}
		}

		private void FillOptions(CellRect rect)
		{
			FillOptions(optionsX, rect.Width);
			FillOptions(optionsZ, rect.Height);
			if (optionsZ.Any((Pair<int, int> x) => x.First > 1))
			{
				optionsX.RemoveAll((Pair<int, int> x) => x.First >= 3 && GetRoomSize(x.First, x.Second, rect.Width) <= 7);
			}
			if (optionsX.Any((Pair<int, int> x) => x.First > 1))
			{
				optionsZ.RemoveAll((Pair<int, int> x) => x.First >= 3 && GetRoomSize(x.First, x.Second, rect.Height) <= 7);
			}
		}

		private void FillOptions(List<Pair<int, int>> outOptions, int length)
		{
			outOptions.Clear();
			for (int i = 2; i <= 4; i++)
			{
				for (int j = 1; j <= 5; j++)
				{
					int roomSize = GetRoomSize(i, j, length);
					if (roomSize != -1 && roomSize >= 6 && roomSize >= 2 * j - 1)
					{
						outOptions.Add(new Pair<int, int>(i, j));
					}
				}
			}
		}

		private int GetRoomSize(int roomsPerRow, int pathwayWidth, int totalLength)
		{
			int num = totalLength - (roomsPerRow - 1) * pathwayWidth;
			if (num % roomsPerRow != 0)
			{
				return -1;
			}
			return num / roomsPerRow;
		}

		private bool TryResolveRandomOption(int maxWidthHeightDiff, int maxPathwayWidthDiff, ResolveParams rp)
		{
			options.Clear();
			for (int i = 0; i < optionsX.Count; i++)
			{
				int first = optionsX[i].First;
				int second = optionsX[i].Second;
				int roomSize = GetRoomSize(first, second, rp.rect.Width);
				for (int j = 0; j < optionsZ.Count; j++)
				{
					int first2 = optionsZ[j].First;
					int second2 = optionsZ[j].Second;
					int roomSize2 = GetRoomSize(first2, second2, rp.rect.Height);
					if (Mathf.Abs(roomSize - roomSize2) <= maxWidthHeightDiff && Mathf.Abs(second - second2) <= maxPathwayWidthDiff)
					{
						options.Add(new Pair<Pair<int, int>, Pair<int, int>>(optionsX[i], optionsZ[j]));
					}
				}
			}
			if (options.Any())
			{
				Pair<Pair<int, int>, Pair<int, int>> pair = options.RandomElement();
				ResolveOption(pair.First.First, pair.First.Second, pair.Second.First, pair.Second.Second, rp);
				return true;
			}
			return false;
		}

		private void ResolveOption(int roomsPerRowX, int pathwayWidthX, int roomsPerRowZ, int pathwayWidthZ, ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			int roomSize = GetRoomSize(roomsPerRowX, pathwayWidthX, rp.rect.Width);
			int roomSize2 = GetRoomSize(roomsPerRowZ, pathwayWidthZ, rp.rect.Height);
			ThingDef thingDef = null;
			if (pathwayWidthX >= 3)
			{
				thingDef = ((rp.faction != null && (int)rp.faction.def.techLevel < 4) ? ThingDefOf.TorchLamp : ThingDefOf.StandingLamp);
			}
			TerrainDef floorDef = rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
			int num = roomSize;
			for (int i = 0; i < roomsPerRowX - 1; i++)
			{
				CellRect rect = new CellRect(rp.rect.minX + num, rp.rect.minZ, pathwayWidthX, rp.rect.Height);
				ResolveParams resolveParams = rp;
				resolveParams.rect = rect;
				resolveParams.floorDef = floorDef;
				resolveParams.streetHorizontal = false;
				BaseGen.symbolStack.Push("street", resolveParams);
				num += roomSize + pathwayWidthX;
			}
			int num2 = roomSize2;
			for (int j = 0; j < roomsPerRowZ - 1; j++)
			{
				CellRect rect2 = new CellRect(rp.rect.minX, rp.rect.minZ + num2, rp.rect.Width, pathwayWidthZ);
				ResolveParams resolveParams2 = rp;
				resolveParams2.rect = rect2;
				resolveParams2.floorDef = floorDef;
				resolveParams2.streetHorizontal = true;
				BaseGen.symbolStack.Push("street", resolveParams2);
				num2 += roomSize2 + pathwayWidthZ;
			}
			num = 0;
			num2 = 0;
			children.Clear();
			for (int k = 0; k < roomsPerRowX; k++)
			{
				for (int l = 0; l < roomsPerRowZ; l++)
				{
					Child child = new Child();
					child.rect = new CellRect(rp.rect.minX + num, rp.rect.minZ + num2, roomSize, roomSize2);
					child.gridX = k;
					child.gridY = l;
					children.Add(child);
					num2 += roomSize2 + pathwayWidthZ;
				}
				num += roomSize + pathwayWidthX;
				num2 = 0;
			}
			MergeRandomChildren();
			children.Shuffle();
			for (int m = 0; m < children.Count; m++)
			{
				if (thingDef != null)
				{
					IntVec3 c = new IntVec3(children[m].rect.maxX + 1, 0, children[m].rect.maxZ);
					if (rp.rect.Contains(c) && c.Standable(map))
					{
						ResolveParams resolveParams3 = rp;
						resolveParams3.rect = CellRect.SingleCell(c);
						resolveParams3.singleThingDef = thingDef;
						BaseGen.symbolStack.Push("thing", resolveParams3);
					}
				}
				ResolveParams resolveParams4 = rp;
				resolveParams4.rect = children[m].rect;
				BaseGen.symbolStack.Push("basePart_outdoors", resolveParams4);
			}
		}

		private void MergeRandomChildren()
		{
			if (children.Count < 4)
			{
				return;
			}
			int num = GenMath.RoundRandom((float)children.Count / 6f);
			for (int i = 0; i < num; i++)
			{
				Child child = children.Find((Child x) => !x.merged);
				if (child != null)
				{
					Child child2 = children.Find((Child x) => x != child && ((Mathf.Abs(x.gridX - child.gridX) == 1 && x.gridY == child.gridY) || (Mathf.Abs(x.gridY - child.gridY) == 1 && x.gridX == child.gridX)));
					if (child2 != null)
					{
						children.Remove(child);
						children.Remove(child2);
						Child child3 = new Child();
						child3.gridX = Mathf.Min(child.gridX, child2.gridX);
						child3.gridY = Mathf.Min(child.gridY, child2.gridY);
						child3.merged = true;
						child3.rect = CellRect.FromLimits(Mathf.Min(child.rect.minX, child2.rect.minX), Mathf.Min(child.rect.minZ, child2.rect.minZ), Mathf.Max(child.rect.maxX, child2.rect.maxX), Mathf.Max(child.rect.maxZ, child2.rect.maxZ));
						children.Add(child3);
					}
					continue;
				}
				break;
			}
		}
	}
}
